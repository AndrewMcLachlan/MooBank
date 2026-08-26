/*
    Demo account top-up, part 3 of 3. Run after new transactions land on the checking account.

    DemoUtilities.sql creates the two utility accounts and a bill per payment, and then refuses to
    run again. This adds a bill for every electricity or water payment on checking that has none,
    picking up each account's period and meter reading where its last bill left off.

    Run it as often as you like: with nothing new on checking it writes nothing.

    NOTE: water bills carry two service charges, which the fixed utilities.TotalCost totals
    correctly. The version that joins service charges to usages counts the consumption once per
    service charge, and every water bill will read high by exactly its usage cost.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

-- All match DemoUtilities.sql, so a topped-up account continues the series rather than stepping.
DECLARE @ElectricitySupplyPerDay DECIMAL(12, 5) = 1.05000;
DECLARE @ElectricityPricePerUnit DECIMAL(7, 5) = 0.29800;
DECLARE @WaterServicePerDay DECIMAL(12, 5) = 0.36000;
DECLARE @SeweragePerDay DECIMAL(12, 5) = 0.47000;
DECLARE @WaterPricePerUnit DECIMAL(7, 5) = 3.35000;

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

/*
    Payments falling on the same day become one bill: a period runs from the previous bill, so a
    second bill issued the same day would have to cover one that begins the day after it ends.

    CROSS APPLY rather than a join to the tags: a transaction split across two tagged splits would
    otherwise appear once per split, each time carrying the whole amount.
*/
DECLARE @Bills TABLE (
    Utility CHAR(1) NOT NULL,
    IssueDate DATE NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    AmountPaid DECIMAL(12, 4) NOT NULL,
    PeriodStart DATE NULL,
    Days INT NULL,
    ServiceTotal DECIMAL(12, 4) NULL,
    Usage DECIMAL(7, 3) NULL,
    PreviousReading INT NULL,
    CurrentReading INT NULL,
    InvoiceNumber VARCHAR(15) NOT NULL,
    PRIMARY KEY (Utility, IssueDate)
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
)
INSERT INTO @Bills (Utility, IssueDate, AccountId, AmountPaid, InvoiceNumber)
SELECT
    p.Utility,
    p.PaidOn,
    CASE WHEN p.Utility = 'E' THEN @ElectricityId ELSE @WaterId END,
    SUM(p.AmountPaid),
    p.Utility + CONVERT(VARCHAR(8), p.PaidOn, 112)
FROM Payments p
WHERE NOT EXISTS (
    SELECT 1 FROM utilities.Bill b
    WHERE b.AccountId = CASE WHEN p.Utility = 'E' THEN @ElectricityId ELSE @WaterId END
      AND b.IssueDate = p.PaidOn)
GROUP BY p.Utility, p.PaidOn;

IF NOT EXISTS (SELECT 1 FROM @Bills)
BEGIN
    PRINT 'Every electricity and water payment on the demo checking account already has a bill. Nothing to do.';
    RETURN;
END

/*
    Each account carries on from its own last bill: the next period opens the day after that bill
    was issued, and the meter continues from its closing reading.
*/
DECLARE @LastElectricity DATE, @LastWater DATE, @ReadingElectricity INT, @ReadingWater INT;

SELECT TOP 1 @LastElectricity = b.IssueDate, @ReadingElectricity = b.CurrentReading
FROM utilities.Bill b WHERE b.AccountId = @ElectricityId ORDER BY b.IssueDate DESC;

SELECT TOP 1 @LastWater = b.IssueDate, @ReadingWater = b.CurrentReading
FROM utilities.Bill b WHERE b.AccountId = @WaterId ORDER BY b.IssueDate DESC;

UPDATE b
SET PeriodStart = ISNULL(DATEADD(DAY, 1, prev.PreviousIssue), DATEADD(DAY, CASE WHEN b.Utility = 'E' THEN -42 ELSE -90 END, b.IssueDate))
FROM @Bills b
CROSS APPLY (
    SELECT COALESCE(
        (SELECT MAX(e.IssueDate) FROM @Bills e WHERE e.Utility = b.Utility AND e.IssueDate < b.IssueDate),
        CASE WHEN b.Utility = 'E' THEN @LastElectricity ELSE @LastWater END) AS PreviousIssue
) prev;

/*
    A period is shortened when its service charges would otherwise swallow most of the bill, which
    keeps the solved consumption positive without letting the rates wander. Only an unusually long
    gap between payments, or an unusually small payment, is affected.
*/
UPDATE b
SET Days = d.Days,
    PeriodStart = DATEADD(DAY, -(d.Days - 1), b.IssueDate),
    ServiceTotal = r.ServicePerDay * d.Days,
    Usage = CAST(ROUND((b.AmountPaid - r.ServicePerDay * d.Days) /
        CASE WHEN b.Utility = 'E' THEN @ElectricityPricePerUnit ELSE @WaterPricePerUnit END, 3) AS DECIMAL(7, 3))
FROM @Bills b
CROSS APPLY (
    SELECT CASE WHEN b.Utility = 'E' THEN @ElectricitySupplyPerDay ELSE @WaterServicePerDay + @SeweragePerDay END AS ServicePerDay,
           DATEDIFF(DAY, b.PeriodStart, b.IssueDate) + 1 AS NaturalDays
) r
CROSS APPLY (
    SELECT CASE
        WHEN r.NaturalDays < 1 THEN 1
        WHEN r.ServicePerDay * r.NaturalDays <= b.AmountPaid * 0.6 THEN r.NaturalDays
        ELSE GREATEST(CAST(FLOOR(b.AmountPaid * 0.6 / r.ServicePerDay) AS INT), 1)
    END AS Days
) d;

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
