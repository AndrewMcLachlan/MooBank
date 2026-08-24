/*
    Demo account backfill, part 4 of 6.

    The demo family owns no utilities accounts, so the electricity and water payments sitting on the
    checking account have no bills behind them and the bill reports are empty. This creates an
    Electricity and a Water account and one bill per existing payment, shaped so that the bill's
    computed Cost reproduces the amount actually paid.

    Each bill carries a period running from the day after the previous bill ended to the payment
    date, the service charges that utility levies -- supply for electricity, water and sewerage for
    water -- and one consumption row. The consumption quantity is solved from the amount paid, so
    the rates stay round numbers and the bill adds up.

    Meter readings run monotonically upward across bills so the usage reports have a sensible series.

    NOTE: water bills carry two service charges, which the fixed utilities.TotalCost in this branch
    is required to total correctly. The version on main joins service charges to usages and so
    counts the consumption once per service charge.

    Idempotent: no-op if the demo family already owns an Electricity account.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

-- Rates are held flat and the consumption solved from the amount paid; the alternative, solving the
-- rate, produces prices like 0.28137 that no retailer would print.
DECLARE @ElectricitySupplyPerDay DECIMAL(12, 5) = 1.10000;
DECLARE @ElectricityPricePerUnit DECIMAL(7, 5) = 0.30000;   -- per kWh
DECLARE @WaterServicePerDay DECIMAL(12, 5) = 0.55000;
DECLARE @SeweragePerDay DECIMAL(12, 5) = 0.75000;
DECLARE @WaterPricePerUnit DECIMAL(7, 5) = 2.95000;         -- per kL

DECLARE @CheckingId UNIQUEIDENTIFIER, @OwnerId UNIQUEIDENTIFIER;

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

SELECT TOP 1 @OwnerId = UserId FROM dbo.InstrumentOwner WHERE InstrumentId = @CheckingId;

IF @OwnerId IS NULL THROW 50000, 'Demo checking account has no owner to copy.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.Instrument i
    INNER JOIN dbo.InstrumentOwner io ON io.InstrumentId = i.Id
    INNER JOIN dbo.[User] u ON u.Id = io.UserId
    WHERE u.FamilyId = @FamilyId AND i.[Name] = N'Electricity')
BEGIN
    PRINT 'Demo utilities accounts already exist. Nothing to do.';
    RETURN;
END

DECLARE @ElectricityId UNIQUEIDENTIFIER = NEWID();
DECLARE @WaterId UNIQUEIDENTIFIER = NEWID();

/*
    One row per bill, keyed by invoice number. The invoice number is derived from the utility and
    the payment date, which makes it both a realistic reference and the key used to thread the
    generated Bill, Period and charge rows back together through their identity columns.
*/
DECLARE @Bills TABLE (
    InvoiceNumber VARCHAR(15) NOT NULL PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Utility CHAR(1) NOT NULL,
    IssueDate DATE NOT NULL,
    PeriodStart DATE NOT NULL,
    PeriodEnd DATE NOT NULL,
    Days INT NOT NULL,
    AmountPaid DECIMAL(12, 4) NOT NULL,
    ServiceTotal DECIMAL(12, 4) NOT NULL,
    Usage DECIMAL(7, 3) NOT NULL,
    PreviousReading INT NOT NULL,
    CurrentReading INT NOT NULL
);

;WITH Payments AS (
    SELECT
        CASE WHEN tg.[Name] = N'Electricity' THEN 'E' ELSE 'W' END AS Utility,
        CAST(t.TransactionTime AS DATE) AS PaidOn,
        ABS(t.Amount) AS AmountPaid,
        LAG(CAST(t.TransactionTime AS DATE)) OVER (
            PARTITION BY tg.[Name] ORDER BY t.TransactionTime, t.TransactionId) AS PreviousPaidOn
    FROM dbo.[Transaction] t
    INNER JOIN dbo.TransactionSplit ts ON ts.TransactionId = t.TransactionId
    INNER JOIN dbo.TransactionSplitTag tst ON tst.TransactionSplitId = ts.Id
    INNER JOIN dbo.Tag tg ON tg.Id = tst.TagId
    WHERE t.AccountId = @CheckingId
      AND tg.FamilyId = @FamilyId
      AND tg.[Name] IN (N'Electricity', N'Water')
),
Periods AS (
    SELECT
        p.Utility,
        p.PaidOn,
        p.AmountPaid,
        -- The first bill has no predecessor, so it is given a period typical of its utility.
        ISNULL(DATEADD(DAY, 1, p.PreviousPaidOn), DATEADD(DAY, CASE WHEN p.Utility = 'E' THEN -42 ELSE -90 END, p.PaidOn)) AS PeriodStart
    FROM Payments p
),
Costed AS (
    SELECT
        pe.*,
        DATEDIFF(DAY, pe.PeriodStart, pe.PaidOn) + 1 AS Days,
        CASE WHEN pe.Utility = 'E'
             THEN @ElectricitySupplyPerDay * (DATEDIFF(DAY, pe.PeriodStart, pe.PaidOn) + 1)
             ELSE (@WaterServicePerDay + @SeweragePerDay) * (DATEDIFF(DAY, pe.PeriodStart, pe.PaidOn) + 1)
        END AS ServiceTotal
    FROM Periods pe
)
INSERT INTO @Bills (InvoiceNumber, AccountId, Utility, IssueDate, PeriodStart, PeriodEnd, Days, AmountPaid, ServiceTotal, Usage, PreviousReading, CurrentReading)
SELECT
    c.Utility + CONVERT(VARCHAR(8), c.PaidOn, 112),
    CASE WHEN c.Utility = 'E' THEN @ElectricityId ELSE @WaterId END,
    c.Utility,
    c.PaidOn,
    c.PeriodStart,
    c.PaidOn,
    c.Days,
    c.AmountPaid,
    c.ServiceTotal,
    Solved.Usage,
    0,
    0
FROM Costed c
CROSS APPLY (
    SELECT CAST(ROUND(
        (c.AmountPaid - c.ServiceTotal) /
        CASE WHEN c.Utility = 'E' THEN @ElectricityPricePerUnit ELSE @WaterPricePerUnit END, 3) AS DECIMAL(7, 3)) AS Usage
) Solved;

IF NOT EXISTS (SELECT 1 FROM @Bills)
    THROW 50000, 'No electricity or water payments found on the demo checking account.', 1;

-- A period whose service charges alone exceed what was paid would need negative consumption.
IF EXISTS (SELECT 1 FROM @Bills WHERE Usage <= 0)
    THROW 50000, 'A bill period produced non-positive consumption. Check the service charge rates.', 1;

-- Meter readings accumulate per utility so the usage series climbs.
UPDATE b
SET PreviousReading = r.Opening + r.RunningBefore,
    CurrentReading = r.Opening + r.RunningBefore + CAST(ROUND(b.Usage, 0) AS INT)
FROM @Bills b
INNER JOIN (
    SELECT
        InvoiceNumber,
        CASE WHEN Utility = 'E' THEN 41200 ELSE 830 END AS Opening,
        CAST(ISNULL(SUM(ROUND(Usage, 0)) OVER (
            PARTITION BY Utility ORDER BY IssueDate
            ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS INT) AS RunningBefore
    FROM @Bills
) r ON r.InvoiceNumber = b.InvoiceNumber;

BEGIN TRAN;

INSERT INTO dbo.Instrument (Id, [Name], [Description], ShareWithFamily, Slug)
VALUES (@ElectricityId, N'Electricity', N'Demo electricity account', 1, 'demo-electricity'),
       (@WaterId, N'Water', N'Demo water account', 1, 'demo-water');

INSERT INTO dbo.InstrumentOwner (InstrumentId, UserId)
VALUES (@ElectricityId, @OwnerId), (@WaterId, @OwnerId);

INSERT INTO utilities.Account (InstrumentId, AccountNumber, UtilityTypeId)
VALUES (@ElectricityId, '4000123456', 1),      -- UtilityType.Electricity
       (@WaterId, '7000654321', 3);            -- UtilityType.Water

DECLARE @BillMap TABLE (BillId INT NOT NULL PRIMARY KEY, InvoiceNumber VARCHAR(15) NOT NULL);

INSERT INTO utilities.Bill (AccountId, InvoiceNumber, IssueDate, CurrentReading, PreviousReading, CostsIncludeGST)
OUTPUT inserted.Id, inserted.InvoiceNumber INTO @BillMap (BillId, InvoiceNumber)
SELECT b.AccountId, b.InvoiceNumber, b.IssueDate, b.CurrentReading, b.PreviousReading, 1
FROM @Bills b;

DECLARE @PeriodMap TABLE (PeriodId INT NOT NULL PRIMARY KEY, BillId INT NOT NULL);

INSERT INTO utilities.Period (BillId, PeriodStart, PeriodEnd)
OUTPUT inserted.Id, inserted.BillId INTO @PeriodMap (PeriodId, BillId)
SELECT m.BillId, b.PeriodStart, b.PeriodEnd
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

INSERT INTO utilities.[Usage] (PeriodId, PricePerUnit, TotalUsage)
SELECT
    p.PeriodId,
    CASE WHEN b.Utility = 'E' THEN @ElectricityPricePerUnit ELSE @WaterPricePerUnit END,
    b.Usage
FROM @PeriodMap p
INNER JOIN @BillMap m ON m.BillId = p.BillId
INNER JOIN @Bills b ON b.InvoiceNumber = m.InvoiceNumber;

COMMIT;

SELECT
    i.[Name] AS Account,
    COUNT(*) AS Bills,
    MIN(bl.IssueDate) AS FirstBill,
    MAX(bl.IssueDate) AS LastBill,
    CAST(MAX(ABS(bl.Cost - b.AmountPaid)) AS DECIMAL(12, 4)) AS WorstCostDifference
FROM utilities.Bill bl
INNER JOIN dbo.Instrument i ON i.Id = bl.AccountId
INNER JOIN @Bills b ON b.InvoiceNumber = bl.InvoiceNumber
WHERE bl.AccountId IN (@ElectricityId, @WaterId)
GROUP BY i.[Name];
