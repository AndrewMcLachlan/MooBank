/*
    Demo account top-up, part 3 of 3. Run after new transactions land on the checking account.

    Adds bills for the electricity and water payments made since each account's last bill, picking
    up its period, meter reading and tariff where that bill left off. Use it when the monthly job
    has missed a month; DemoUtilitiesRebuild.sql is the one that lays down the history.

    Run it as often as you like: with nothing new on checking it writes nothing.

    NOTE: water bills carry two service charges, which the fixed utilities.TotalCost totals
    correctly. The version that joins service charges to usages counts the consumption once per
    service charge, and every water bill will read high by exactly its usage cost.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

/*
    The tariff is not declared here. It is read back off each account's most recent bill, below, so
    a top-up continues whatever the data already says -- including a retailer change or a price rise
    that this script knows nothing about.
*/
DECLARE @ElectricitySupplyPerDay DECIMAL(12, 5), @ElectricityPricePerUnit DECIMAL(7, 5);
DECLARE @WaterServicePerDay DECIMAL(12, 5), @SeweragePerDay DECIMAL(12, 5), @WaterPricePerUnit DECIMAL(7, 5);

DECLARE @CheckingId UNIQUEIDENTIFIER, @ElectricityId UNIQUEIDENTIFIER, @WaterId UNIQUEIDENTIFIER;

SELECT @CheckingId = f.Id
FROM (
    SELECT i.[Name], MIN(i.Id) AS Id, COUNT(*) AS Matches
    FROM dbo.Instrument i
    WHERE EXISTS (
        SELECT 1 FROM dbo.InstrumentOwner io
        INNER JOIN dbo.[User] u ON u.Id = io.UserId
        WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
    GROUP BY i.[Name]
) f WHERE f.[Name] = N'Checking Account' AND f.Matches = 1;

IF @CheckingId IS NULL THROW 50000, 'Demo checking account not found, or matched more than once.', 1;

/*
    The accounts are named after their retailer and the household changes electricity retailer every
    few years, so they are found by what they are rather than what they are called: the open account
    of each utility type. A closed one is a retailer they have left.
*/
SELECT TOP 1 @ElectricityId = a.InstrumentId
FROM utilities.Account a
INNER JOIN dbo.Instrument i ON i.Id = a.InstrumentId
WHERE a.UtilityTypeId = 1 AND i.ClosedDate IS NULL
  AND EXISTS (SELECT 1 FROM dbo.InstrumentOwner io INNER JOIN dbo.[User] u ON u.Id = io.UserId
              WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
ORDER BY i.[Name];

SELECT TOP 1 @WaterId = a.InstrumentId
FROM utilities.Account a
INNER JOIN dbo.Instrument i ON i.Id = a.InstrumentId
WHERE a.UtilityTypeId = 3 AND i.ClosedDate IS NULL
  AND EXISTS (SELECT 1 FROM dbo.InstrumentOwner io INNER JOIN dbo.[User] u ON u.Id = io.UserId
              WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
ORDER BY i.[Name];

IF @ElectricityId IS NULL OR @WaterId IS NULL
    THROW 50000, 'No open demo electricity or water account. Run DemoUtilitiesRebuild.sql first.', 1;

DECLARE @ClusterDays INT = 21;

/*
    The tariff comes off each account's most recent bill: its service charges, and the price of its
    consumption. Nothing here needs to know what a kilowatt hour costs this year.
*/
DECLARE @LastElectricity DATE, @LastWater DATE, @ReadingElectricity INT, @ReadingWater INT;

SELECT TOP 1 @LastElectricity = b.IssueDate, @ReadingElectricity = b.CurrentReading
FROM utilities.Bill b WHERE b.AccountId = @ElectricityId ORDER BY b.IssueDate DESC, b.Id DESC;

SELECT TOP 1 @LastWater = b.IssueDate, @ReadingWater = b.CurrentReading
FROM utilities.Bill b WHERE b.AccountId = @WaterId ORDER BY b.IssueDate DESC, b.Id DESC;

IF @LastElectricity IS NULL OR @LastWater IS NULL
    THROW 50000, 'A demo utility account has no bills to take a tariff from. Run DemoUtilitiesRebuild.sql first.', 1;

SELECT @ElectricitySupplyPerDay = SUM(sc.ChargePerDay)
FROM utilities.Bill b
INNER JOIN utilities.Period p ON p.BillId = b.Id
INNER JOIN utilities.ServiceCharge sc ON sc.PeriodId = p.Id
WHERE b.AccountId = @ElectricityId AND b.IssueDate = @LastElectricity;

SELECT TOP 1 @ElectricityPricePerUnit = u.PricePerUnit
FROM utilities.Bill b
INNER JOIN utilities.Period p ON p.BillId = b.Id
INNER JOIN utilities.[Usage] u ON u.PeriodId = p.Id
WHERE b.AccountId = @ElectricityId AND b.IssueDate = @LastElectricity AND ISNULL(u.UsageTypeId, 1) = 1;

SELECT
    @WaterServicePerDay = SUM(CASE WHEN sc.ChargeTypeId = 3 THEN 0 ELSE sc.ChargePerDay END),
    @SeweragePerDay = SUM(CASE WHEN sc.ChargeTypeId = 3 THEN sc.ChargePerDay ELSE 0 END)
FROM utilities.Bill b
INNER JOIN utilities.Period p ON p.BillId = b.Id
INNER JOIN utilities.ServiceCharge sc ON sc.PeriodId = p.Id
WHERE b.AccountId = @WaterId AND b.IssueDate = @LastWater;

SELECT TOP 1 @WaterPricePerUnit = u.PricePerUnit
FROM utilities.Bill b
INNER JOIN utilities.Period p ON p.BillId = b.Id
INNER JOIN utilities.[Usage] u ON u.PeriodId = p.Id
WHERE b.AccountId = @WaterId AND b.IssueDate = @LastWater AND ISNULL(u.UsageTypeId, 1) = 1;

IF @ElectricityPricePerUnit IS NULL OR @WaterPricePerUnit IS NULL
    THROW 50000, 'The last demo utility bill has no priced consumption to copy.', 1;

/*
    One bill per cluster of payments, not one per payment date. The generator pays electricity in
    bursts, and billing each of them separately gives a bill covering a day or two while carrying a
    whole cycle's consumption.

    CROSS APPLY rather than a join to the tags: a transaction split across two tagged splits would
    otherwise appear once per split, each time carrying the whole amount.
*/
DECLARE @Bills TABLE (
    Utility CHAR(1) NOT NULL,
    BillNo INT NOT NULL,
    IssueDate DATE NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    AmountPaid DECIMAL(12, 4) NOT NULL,
    PeriodStart DATE NULL,
    Days INT NULL,
    ServiceTotal DECIMAL(12, 4) NULL,
    Usage DECIMAL(7, 3) NULL,
    PreviousReading INT NULL,
    CurrentReading INT NULL,
    InvoiceNumber AS Utility + CONVERT(VARCHAR(8), IssueDate, 112),
    PRIMARY KEY (Utility, BillNo)
);

;WITH Payments AS (
    SELECT
        CASE WHEN u.[Name] = N'Electricity' THEN 'E' ELSE 'W' END AS Utility,
        CAST(t.TransactionTime AS DATE) AS PaidOn,
        ABS(t.Amount) AS AmountPaid
    FROM dbo.[Transaction] t
    CROSS APPLY (
        SELECT TOP 1 tg.[Name]
        FROM dbo.TransactionSplit ts
        INNER JOIN dbo.TransactionSplitTag tst ON tst.TransactionSplitId = ts.Id
        INNER JOIN dbo.Tag tg ON tg.Id = tst.TagId
        WHERE ts.TransactionId = t.TransactionId
          AND tg.FamilyId = @FamilyId
          AND tg.[Name] IN (N'Electricity', N'Water')
        ORDER BY tg.[Name]
    ) u
    WHERE t.AccountId = @CheckingId
),
Unbilled AS (
    SELECT Utility, PaidOn, SUM(AmountPaid) AS AmountPaid
    FROM Payments p
    WHERE p.PaidOn > CASE WHEN p.Utility = 'E' THEN @LastElectricity ELSE @LastWater END
    GROUP BY Utility, PaidOn
),
Marked AS (
    SELECT
        Utility, PaidOn, AmountPaid,
        CASE WHEN DATEDIFF(DAY, LAG(PaidOn) OVER (PARTITION BY Utility ORDER BY PaidOn), PaidOn) > @ClusterDays
               OR LAG(PaidOn) OVER (PARTITION BY Utility ORDER BY PaidOn) IS NULL
             THEN 1 ELSE 0 END AS StartsBill
    FROM Unbilled
),
Grouped AS (
    SELECT Utility, PaidOn, AmountPaid,
           SUM(StartsBill) OVER (PARTITION BY Utility ORDER BY PaidOn ROWS UNBOUNDED PRECEDING) AS BillNo
    FROM Marked
)
INSERT INTO @Bills (Utility, BillNo, IssueDate, AccountId, AmountPaid)
SELECT Utility, BillNo, MAX(PaidOn), CASE WHEN Utility = 'E' THEN @ElectricityId ELSE @WaterId END, SUM(AmountPaid)
FROM Grouped
GROUP BY Utility, BillNo;

IF NOT EXISTS (SELECT 1 FROM @Bills)
BEGIN
    PRINT 'Every electricity and water payment on the demo checking account already has a bill. Nothing to do.';
    RETURN;
END

-- A period opens the day after the bill before it, the first carrying on from the account's own
-- last bill.
UPDATE b
SET PeriodStart = DATEADD(DAY, 1, COALESCE(prev.IssueDate, CASE WHEN b.Utility = 'E' THEN @LastElectricity ELSE @LastWater END))
FROM @Bills b
OUTER APPLY (SELECT MAX(p.IssueDate) AS IssueDate FROM @Bills p WHERE p.Utility = b.Utility AND p.BillNo = b.BillNo - 1) prev;

UPDATE b
SET Days = DATEDIFF(DAY, b.PeriodStart, b.IssueDate) + 1,
    ServiceTotal = r.ServicePerDay * (DATEDIFF(DAY, b.PeriodStart, b.IssueDate) + 1),
    Usage = CAST(ROUND((b.AmountPaid - r.ServicePerDay * (DATEDIFF(DAY, b.PeriodStart, b.IssueDate) + 1)) /
        CASE WHEN b.Utility = 'E' THEN @ElectricityPricePerUnit ELSE @WaterPricePerUnit END, 3) AS DECIMAL(7, 3))
FROM @Bills b
CROSS APPLY (
    SELECT CASE WHEN b.Utility = 'E' THEN @ElectricitySupplyPerDay ELSE @WaterServicePerDay + @SeweragePerDay END AS ServicePerDay
) r;

IF EXISTS (SELECT 1 FROM @Bills WHERE Days < 1)
    THROW 50000, 'A bill period came out shorter than a day. Check @ClusterDays.', 1;

IF EXISTS (SELECT 1 FROM @Bills WHERE Usage <= 0)
    THROW 50000, 'A bill period produced non-positive consumption. Check the service charge rates.', 1;

UPDATE b
SET PreviousReading = r.Opening + r.RunningBefore,
    CurrentReading = r.Opening + r.RunningBefore + CAST(ROUND(b.Usage, 0) AS INT)
FROM @Bills b
INNER JOIN (
    SELECT
        Utility,
        IssueDate,
        CASE WHEN Utility = 'E' THEN ISNULL(@ReadingElectricity, 41200) ELSE ISNULL(@ReadingWater, 830) END AS Opening,
        CAST(ISNULL(SUM(ROUND(Usage, 0)) OVER (
            PARTITION BY Utility ORDER BY IssueDate
            ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS INT) AS RunningBefore
    FROM @Bills
) r ON r.Utility = b.Utility AND r.IssueDate = b.IssueDate;

BEGIN TRAN;

DECLARE @BillMap TABLE (BillId INT NOT NULL PRIMARY KEY, InvoiceNumber VARCHAR(15) NOT NULL);

INSERT INTO utilities.Bill (AccountId, InvoiceNumber, IssueDate, CurrentReading, PreviousReading, CostsIncludeGST)
OUTPUT inserted.Id, inserted.InvoiceNumber INTO @BillMap (BillId, InvoiceNumber)
SELECT b.AccountId, b.InvoiceNumber, b.IssueDate, b.CurrentReading, b.PreviousReading, 1
FROM @Bills b;

DECLARE @PeriodMap TABLE (PeriodId INT NOT NULL PRIMARY KEY, BillId INT NOT NULL);

INSERT INTO utilities.Period (BillId, PeriodStart, PeriodEnd)
OUTPUT inserted.Id, inserted.BillId INTO @PeriodMap (PeriodId, BillId)
SELECT m.BillId, b.PeriodStart, b.IssueDate
FROM @BillMap m
INNER JOIN @Bills b ON b.InvoiceNumber = m.InvoiceNumber;

INSERT INTO utilities.ServiceCharge (PeriodId, ChargePerDay, ChargeTypeId)
SELECT p.PeriodId, c.ChargePerDay, c.ChargeTypeId
FROM @PeriodMap p
INNER JOIN @BillMap m ON m.BillId = p.BillId
INNER JOIN @Bills b ON b.InvoiceNumber = m.InvoiceNumber
CROSS APPLY (
    SELECT @ElectricitySupplyPerDay AS ChargePerDay, 1 AS ChargeTypeId WHERE b.Utility = 'E'   -- Supply
    UNION ALL
    SELECT @WaterServicePerDay, 2 WHERE b.Utility = 'W'                                        -- Water Service
    UNION ALL
    SELECT @SeweragePerDay, 3 WHERE b.Utility = 'W'                                            -- Sewerage Service
) c;

-- Consumption, stated rather than left to default: the column is nullable until the follow-up
-- tightens it, and a null read back through the non-nullable UsageType throws.
INSERT INTO utilities.[Usage] (PeriodId, PricePerUnit, TotalUsage, UsageTypeId)
SELECT
    p.PeriodId,
    CASE WHEN b.Utility = 'E' THEN @ElectricityPricePerUnit ELSE @WaterPricePerUnit END,
    b.Usage,
    1                                                        -- UsageType.Consumption
FROM @PeriodMap p
INNER JOIN @BillMap m ON m.BillId = p.BillId
INNER JOIN @Bills b ON b.InvoiceNumber = m.InvoiceNumber;

COMMIT;

SELECT
    CASE WHEN b.Utility = 'E' THEN 'Electricity' ELSE 'Water' END AS Account,
    COUNT(*) AS BillsAdded,
    MIN(b.IssueDate) AS FirstAdded,
    MAX(b.IssueDate) AS LastAdded,
    CAST(MAX(ABS(bl.Cost - b.AmountPaid)) AS DECIMAL(12, 4)) AS WorstCostDifference
FROM @Bills b
INNER JOIN utilities.Bill bl ON bl.InvoiceNumber = b.InvoiceNumber AND bl.AccountId = b.AccountId
GROUP BY b.Utility;
