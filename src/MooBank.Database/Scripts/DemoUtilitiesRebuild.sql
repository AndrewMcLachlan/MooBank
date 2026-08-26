/*
    Demo account rebuild: utility bills.

    Replaces everything DemoUtilities.sql and DemoUtilitiesTopUp.sql produced. Three things were
    wrong with it:

      1. Each bill's period ran from the previous payment, and the generator pays electricity in
         clusters -- 48 of 100 payments land within 20 days of the one before. So half the bills
         covered a day or two while carrying a quarter's consumption: 1,655 kWh on 10 Jul 2014.
      2. There was one account, called "Electricity", that never changed its prices in twelve years.
      3. Cost was pinned to the checking payment, so a cheaper tariff could only ever show up as
         higher consumption, never as a cheaper bill.

    This inverts the derivation. Bills are generated from a tariff and a consumption profile, and
    the checking payments are then set to what the bills come to. Electricity is contestable, so the
    household changes retailer three times and each switch steps the price down before it starts
    creeping up again; water is a monopoly, so it keeps one provider whose prices only rise.

    Household consumption is scaled so that twelve years of generated bills total exactly what the
    checking account already paid. The account balance is therefore unchanged, and the tariffs stay
    honest rather than being bent to hit a number.

    DESTRUCTIVE: deletes the demo family's existing utility accounts and every bill on them. It
    touches nothing outside that family. Re-runnable -- it rebuilds from scratch each time.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
-- Bill.Cost is a computed column over a user-defined function, and writing to such a table requires
-- this. SSMS sets it; sqlcmd does not unless invoked with -I.
SET QUOTED_IDENTIFIER ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

-- Payments this far apart start a new bill. Below it they are the same bill paid in instalments,
-- which is what the clusters in the generated data amount to.
DECLARE @ClusterDays INT = 21;

-- Starting points only: both are scaled below so the bills total what checking actually paid.
DECLARE @ElectricityDailyKwh DECIMAL(9, 4) = 17.0;
DECLARE @WaterDailyKl DECIMAL(9, 4) = 0.55;

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

/*
    Retailers and their prices. Within a retailer the price rises each year; at a switch the new
    retailer starts below where the old one had reached, which is why anyone switches. Across the
    twelve years the unit rate still climbs from 24.5c to 29.8c, so the saving is a step down out of
    a rising trend rather than a decline.

    Fictional names -- no association with any real retailer is intended.
*/
DECLARE @Tariff TABLE (
    Utility CHAR(1) NOT NULL,
    Retailer NVARCHAR(50) NOT NULL,
    ValidFrom DATE NOT NULL,
    SupplyPerDay DECIMAL(12, 5) NOT NULL,
    SeweragePerDay DECIMAL(12, 5) NULL,
    RatePerUnit DECIMAL(7, 5) NOT NULL,
    PRIMARY KEY (Utility, ValidFrom)
);

INSERT INTO @Tariff (Utility, Retailer, ValidFrom, SupplyPerDay, SeweragePerDay, RatePerUnit)
VALUES
    -- Wattle Grid Energy
    ('E', N'Wattle Grid Energy',  '2014-01-01', 0.88000, NULL, 0.24500),
    ('E', N'Wattle Grid Energy',  '2015-07-01', 0.91000, NULL, 0.25200),
    ('E', N'Wattle Grid Energy',  '2016-07-01', 0.94000, NULL, 0.26000),
    ('E', N'Wattle Grid Energy',  '2017-01-01', 0.97000, NULL, 0.26800),
    -- switched
    ('E', N'Kookaburra Power',    '2017-07-01', 0.89000, NULL, 0.24800),
    ('E', N'Kookaburra Power',    '2018-07-01', 0.92000, NULL, 0.25600),
    ('E', N'Kookaburra Power',    '2019-07-01', 0.95000, NULL, 0.26400),
    ('E', N'Kookaburra Power',    '2020-07-01', 0.98000, NULL, 0.27200),
    -- switched
    ('E', N'Redgum Energy',       '2020-10-01', 0.90000, NULL, 0.25200),
    ('E', N'Redgum Energy',       '2021-07-01', 0.93000, NULL, 0.26000),
    ('E', N'Redgum Energy',       '2022-07-01', 0.96000, NULL, 0.27100),
    ('E', N'Redgum Energy',       '2023-07-01', 1.02000, NULL, 0.28800),
    -- switched
    ('E', N'Silverleaf Power',    '2023-12-01', 0.94000, NULL, 0.26800),
    ('E', N'Silverleaf Power',    '2024-07-01', 0.98000, NULL, 0.27900),
    ('E', N'Silverleaf Power',    '2025-07-01', 1.02000, NULL, 0.28900),
    ('E', N'Silverleaf Power',    '2026-07-01', 1.05000, NULL, 0.29800),
    -- Water is a monopoly: one provider, prices only ever rise.
    ('W', N'Kurrajong Water',     '2014-01-01', 0.28000, 0.36000, 2.55000),
    ('W', N'Kurrajong Water',     '2016-07-01', 0.30000, 0.38000, 2.68000),
    ('W', N'Kurrajong Water',     '2018-07-01', 0.31000, 0.40000, 2.81000),
    ('W', N'Kurrajong Water',     '2020-07-01', 0.32000, 0.42000, 2.95000),
    ('W', N'Kurrajong Water',     '2022-07-01', 0.34000, 0.44000, 3.10000),
    ('W', N'Kurrajong Water',     '2024-07-01', 0.35000, 0.45000, 3.22000),
    ('W', N'Kurrajong Water',     '2026-07-01', 0.36000, 0.47000, 3.35000);

/*
    One bill per cluster of payments. Gaps-and-islands: a gap wider than @ClusterDays opens a new
    bill, and everything paid inside the gap belongs to the one before it.
*/
DECLARE @Bills TABLE (
    Utility CHAR(1) NOT NULL,
    BillNo INT NOT NULL,
    IssueDate DATE NOT NULL,
    PaidTotal DECIMAL(12, 4) NOT NULL,
    PeriodStart DATE NULL,
    Days INT NULL,
    Retailer NVARCHAR(50) NULL,
    SupplyPerDay DECIMAL(12, 5) NULL,
    SeweragePerDay DECIMAL(12, 5) NULL,
    RatePerUnit DECIMAL(7, 5) NULL,
    Usage DECIMAL(12, 4) NULL,
    Cost DECIMAL(12, 4) NULL,
    PreviousReading INT NULL,
    CurrentReading INT NULL,
    InvoiceNumber AS Utility + CONVERT(VARCHAR(8), IssueDate, 112),
    PRIMARY KEY (Utility, BillNo)
);

;WITH Payments AS (
    -- CROSS APPLY rather than a join to the tags: a transaction split across two tagged splits
    -- would otherwise appear once per split, each time carrying the whole amount.
    SELECT
        CASE WHEN u.[Name] = N'Electricity' THEN 'E' ELSE 'W' END AS Utility,
        CAST(t.TransactionTime AS DATE) AS PaidOn,
        ABS(t.Amount) AS Amount
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
Daily AS (
    SELECT Utility, PaidOn, SUM(Amount) AS Amount
    FROM Payments
    GROUP BY Utility, PaidOn
),
Marked AS (
    SELECT
        Utility, PaidOn, Amount,
        CASE WHEN DATEDIFF(DAY, LAG(PaidOn) OVER (PARTITION BY Utility ORDER BY PaidOn), PaidOn) > @ClusterDays
               OR LAG(PaidOn) OVER (PARTITION BY Utility ORDER BY PaidOn) IS NULL
             THEN 1 ELSE 0 END AS StartsBill
    FROM Daily
),
Grouped AS (
    SELECT
        Utility, PaidOn, Amount,
        SUM(StartsBill) OVER (PARTITION BY Utility ORDER BY PaidOn ROWS UNBOUNDED PRECEDING) AS BillNo
    FROM Marked
)
INSERT INTO @Bills (Utility, BillNo, IssueDate, PaidTotal)
SELECT Utility, BillNo, MAX(PaidOn), SUM(Amount)
FROM Grouped
GROUP BY Utility, BillNo;

IF NOT EXISTS (SELECT 1 FROM @Bills)
    THROW 50000, 'No electricity or water payments found on the demo checking account.', 1;

-- A period runs from the day after the previous bill was issued. The first of each utility is given
-- a quarter, there being nothing before it.
UPDATE b
SET PeriodStart = ISNULL(DATEADD(DAY, 1, prev.IssueDate), DATEADD(DAY, -90, b.IssueDate))
FROM @Bills b
OUTER APPLY (SELECT MAX(p.IssueDate) AS IssueDate FROM @Bills p WHERE p.Utility = b.Utility AND p.BillNo = b.BillNo - 1) prev;

UPDATE @Bills SET Days = DATEDIFF(DAY, PeriodStart, IssueDate) + 1;

IF EXISTS (SELECT 1 FROM @Bills WHERE Days < 1)
    THROW 50000, 'A bill period came out shorter than a day. Check @ClusterDays.', 1;

UPDATE b
SET Retailer = t.Retailer, SupplyPerDay = t.SupplyPerDay, SeweragePerDay = t.SeweragePerDay, RatePerUnit = t.RatePerUnit
FROM @Bills b
CROSS APPLY (
    SELECT TOP 1 x.Retailer, x.SupplyPerDay, x.SeweragePerDay, x.RatePerUnit
    FROM @Tariff x
    WHERE x.Utility = b.Utility AND x.ValidFrom <= b.IssueDate
    ORDER BY x.ValidFrom DESC
) t;

/*
    Consumption before scaling: a daily baseline shaped by the season at the middle of the period.
    Australian seasons -- electricity peaks over summer on air conditioning and again, less sharply,
    over winter; water peaks over summer on the garden and falls away in the wet.
*/
UPDATE b
SET Usage = CASE b.Utility WHEN 'E' THEN @ElectricityDailyKwh ELSE @WaterDailyKl END * b.Days *
    CASE
        WHEN b.Utility = 'E' THEN
            CASE WHEN m.Mid IN (12, 1, 2) THEN 1.25
                 WHEN m.Mid IN (6, 7, 8) THEN 1.15
                 WHEN m.Mid IN (3, 11) THEN 1.00
                 ELSE 0.85 END
        ELSE
            CASE WHEN m.Mid IN (12, 1, 2) THEN 1.35
                 WHEN m.Mid IN (6, 7, 8) THEN 0.75
                 WHEN m.Mid IN (3, 11) THEN 1.05
                 ELSE 0.90 END
    END
FROM @Bills b
CROSS APPLY (SELECT MONTH(DATEADD(DAY, b.Days / 2, b.PeriodStart)) AS Mid) m;

/*
    Scale consumption so that the bills total exactly what checking paid.

    Only the metered part is scaled: the daily charges are fixed by the tariff and the number of
    days, so the usage has to absorb the whole difference. Doing it the other way -- scaling the
    cost -- would mean the printed rate no longer multiplied out to the printed total.
*/
DECLARE @ElectricityTarget DECIMAL(18, 4) = (SELECT SUM(PaidTotal) FROM @Bills WHERE Utility = 'E');
DECLARE @WaterTarget DECIMAL(18, 4) = (SELECT SUM(PaidTotal) FROM @Bills WHERE Utility = 'W');

DECLARE @ElectricityScale DECIMAL(18, 9), @WaterScale DECIMAL(18, 9);

SELECT @ElectricityScale =
    (@ElectricityTarget - SUM(SupplyPerDay * Days)) / NULLIF(SUM(RatePerUnit * Usage), 0)
FROM @Bills WHERE Utility = 'E';

SELECT @WaterScale =
    (@WaterTarget - SUM((SupplyPerDay + SeweragePerDay) * Days)) / NULLIF(SUM(RatePerUnit * Usage), 0)
FROM @Bills WHERE Utility = 'W';

IF @ElectricityScale IS NULL OR @ElectricityScale <= 0 OR @WaterScale IS NULL OR @WaterScale <= 0
    THROW 50000, 'The daily charges alone exceed what was paid. Lower the supply charges in @Tariff.', 1;

UPDATE @Bills SET Usage = ROUND(Usage * CASE Utility WHEN 'E' THEN @ElectricityScale ELSE @WaterScale END, 3);

UPDATE @Bills
SET Cost = ROUND(ISNULL(SupplyPerDay, 0) * Days + ISNULL(SeweragePerDay, 0) * Days + RatePerUnit * Usage, 2);

IF EXISTS (SELECT 1 FROM @Bills WHERE Usage <= 0)
    THROW 50000, 'A bill came out with non-positive consumption.', 1;

-- Meter readings accumulate per utility so the usage reports have a rising series.
UPDATE b
SET PreviousReading = r.Opening + r.Before,
    CurrentReading = r.Opening + r.Before + CAST(ROUND(b.Usage, 0) AS INT)
FROM @Bills b
INNER JOIN (
    SELECT
        Utility, BillNo,
        CASE WHEN Utility = 'E' THEN 41200 ELSE 830 END AS Opening,
        CAST(ISNULL(SUM(ROUND(Usage, 0)) OVER (PARTITION BY Utility ORDER BY BillNo ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS INT) AS Before
    FROM @Bills
) r ON r.Utility = b.Utility AND r.BillNo = b.BillNo;

BEGIN TRAN;

/*
    Clear out what the earlier scripts built. Scoped to the demo family's utility accounts, in
    foreign-key order.
*/
DECLARE @OldAccounts TABLE (InstrumentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY);

INSERT INTO @OldAccounts (InstrumentId)
SELECT a.InstrumentId
FROM utilities.Account a
INNER JOIN dbo.Instrument i ON i.Id = a.InstrumentId
WHERE EXISTS (
    SELECT 1 FROM dbo.InstrumentOwner io
    INNER JOIN dbo.[User] u ON u.Id = io.UserId
    WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId);

DELETE u FROM utilities.[Usage] u
INNER JOIN utilities.Period p ON p.Id = u.PeriodId
INNER JOIN utilities.Bill b ON b.Id = p.BillId
WHERE b.AccountId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE s FROM utilities.ServiceCharge s
INNER JOIN utilities.Period p ON p.Id = s.PeriodId
INNER JOIN utilities.Bill b ON b.Id = p.BillId
WHERE b.AccountId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE p FROM utilities.Period p
INNER JOIN utilities.Bill b ON b.Id = p.BillId
WHERE b.AccountId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE db FROM utilities.DiscountBill db
INNER JOIN utilities.Bill b ON b.Id = db.BillId
WHERE b.AccountId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE FROM utilities.Bill WHERE AccountId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE FROM utilities.Account WHERE InstrumentId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE FROM dbo.InstrumentOwner WHERE InstrumentId IN (SELECT InstrumentId FROM @OldAccounts);

DELETE FROM dbo.Instrument WHERE Id IN (SELECT InstrumentId FROM @OldAccounts);

/*
    One account per retailer. An electricity account is closed off the day before its successor
    starts, so the dashboard shows which one is current.
*/
DECLARE @Accounts TABLE (
    Utility CHAR(1) NOT NULL,
    Retailer NVARCHAR(50) NOT NULL PRIMARY KEY,
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    FirstBill DATE NOT NULL,
    LastBill DATE NOT NULL,
    ClosedDate DATE NULL
);

INSERT INTO @Accounts (Utility, Retailer, InstrumentId, FirstBill, LastBill)
SELECT Utility, Retailer, NEWID(), MIN(IssueDate), MAX(IssueDate)
FROM @Bills
GROUP BY Utility, Retailer;

UPDATE a
SET ClosedDate = nxt.NextStart
FROM @Accounts a
OUTER APPLY (
    SELECT MIN(n.FirstBill) AS NextStart
    FROM @Accounts n
    WHERE n.Utility = a.Utility AND n.FirstBill > a.LastBill
) nxt
WHERE nxt.NextStart IS NOT NULL;

INSERT INTO dbo.Instrument (Id, [Name], [Description], ControllerId, ShareWithFamily, Slug, ClosedDate)
SELECT
    a.InstrumentId,
    a.Retailer,
    CASE a.Utility WHEN 'E' THEN N'Electricity retailer' ELSE N'Water and sewerage' END,
    0,                                                  -- Controller.Manual
    1,
    LOWER(REPLACE(a.Retailer, ' ', '-')),
    a.ClosedDate
FROM @Accounts a;

INSERT INTO dbo.InstrumentOwner (InstrumentId, UserId)
SELECT a.InstrumentId, @OwnerId FROM @Accounts a;

INSERT INTO utilities.Account (InstrumentId, AccountNumber, UtilityTypeId)
SELECT
    a.InstrumentId,
    CASE a.Utility WHEN 'E' THEN '4' ELSE '7' END + RIGHT('000000000' + CAST(ABS(CHECKSUM(a.Retailer)) % 1000000000 AS VARCHAR(9)), 9),
    CASE a.Utility WHEN 'E' THEN 1 ELSE 3 END           -- UtilityType.Electricity / Water
FROM @Accounts a;

DECLARE @BillMap TABLE (BillId INT NOT NULL PRIMARY KEY, InvoiceNumber VARCHAR(15) NOT NULL);

INSERT INTO utilities.Bill (AccountId, InvoiceNumber, IssueDate, CurrentReading, PreviousReading, CostsIncludeGST)
OUTPUT inserted.Id, inserted.InvoiceNumber INTO @BillMap (BillId, InvoiceNumber)
SELECT a.InstrumentId, b.InvoiceNumber, b.IssueDate, b.CurrentReading, b.PreviousReading, 1
FROM @Bills b
INNER JOIN @Accounts a ON a.Retailer = b.Retailer;

DECLARE @PeriodMap TABLE (PeriodId INT NOT NULL PRIMARY KEY, BillId INT NOT NULL);

-- OUTPUT on an INSERT ... SELECT can only see the inserted row, so the invoice number is picked up
-- afterwards by joining back through @BillMap.
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
    SELECT b.SupplyPerDay AS ChargePerDay, 1 AS ChargeTypeId WHERE b.Utility = 'E'   -- Supply
    UNION ALL
    SELECT b.SupplyPerDay, 2 WHERE b.Utility = 'W'                                   -- Water Service
    UNION ALL
    SELECT b.SeweragePerDay, 3 WHERE b.Utility = 'W'                                 -- Sewerage Service
) c;

-- Consumption, stated rather than left to default: UsageTypeId is nullable until the follow-up
-- tightens it, and a null read back through the non-nullable UsageType throws.
INSERT INTO utilities.[Usage] (PeriodId, PricePerUnit, TotalUsage, UsageTypeId)
SELECT p.PeriodId, b.RatePerUnit, b.Usage, 1                  -- UsageType.Consumption
FROM @PeriodMap p
INNER JOIN @BillMap m ON m.BillId = p.BillId
INNER JOIN @Bills b ON b.InvoiceNumber = m.InvoiceNumber;

/*
    Bring the checking payments into line with the bills they pay. A bill covering several payments
    splits its cost across them in the proportions they were already in, so the spending pattern on
    the account is unchanged in shape; the last payment of each bill absorbs the rounding.

    The split carries the amount as a positive magnitude and the reports read it rather than the
    transaction, so it has to move too.
*/
DECLARE @Repriced TABLE (TransactionId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, NewAmount DECIMAL(12, 4) NOT NULL);

;WITH Tagged AS (
    SELECT
        t.TransactionId,
        CASE WHEN u.[Name] = N'Electricity' THEN 'E' ELSE 'W' END AS Utility,
        CAST(t.TransactionTime AS DATE) AS PaidOn,
        ABS(t.Amount) AS Amount
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
Assigned AS (
    SELECT
        tg.TransactionId, tg.Amount, b.Utility, b.BillNo, b.Cost, b.PaidTotal,
        ROW_NUMBER() OVER (PARTITION BY b.Utility, b.BillNo ORDER BY tg.PaidOn DESC, tg.TransactionId) AS Rn
    FROM Tagged tg
    INNER JOIN @Bills b ON b.Utility = tg.Utility AND tg.PaidOn BETWEEN b.PeriodStart AND b.IssueDate
),
Shared AS (
    SELECT
        a.TransactionId, a.Utility, a.BillNo, a.Cost, a.Rn,
        ROUND(a.Cost * a.Amount / NULLIF(a.PaidTotal, 0), 2) AS Share,
        SUM(ROUND(a.Cost * a.Amount / NULLIF(a.PaidTotal, 0), 2)) OVER (PARTITION BY a.Utility, a.BillNo) AS ShareTotal
    FROM Assigned a
)
INSERT INTO @Repriced (TransactionId, NewAmount)
SELECT s.TransactionId, CASE WHEN s.Rn = 1 THEN s.Share + (s.Cost - s.ShareTotal) ELSE s.Share END
FROM Shared s;

UPDATE t
SET Amount = -r.NewAmount
FROM dbo.[Transaction] t
INNER JOIN @Repriced r ON r.TransactionId = t.TransactionId;

UPDATE ts
SET Amount = r.NewAmount
FROM dbo.TransactionSplit ts
INNER JOIN @Repriced r ON r.TransactionId = ts.TransactionId;

COMMIT;

SELECT
    CASE Utility WHEN 'E' THEN 'Electricity' ELSE 'Water' END AS Utility,
    Retailer,
    COUNT(*) AS Bills,
    MIN(IssueDate) AS FirstBill,
    MAX(IssueDate) AS LastBill,
    MIN(Days) AS ShortestPeriod,
    MAX(Days) AS LongestPeriod,
    CAST(AVG(Cost) AS DECIMAL(12, 2)) AS AverageBill,
    CAST(AVG(RatePerUnit) AS DECIMAL(7, 5)) AS AverageRate
FROM @Bills
GROUP BY Utility, Retailer
ORDER BY Utility, MIN(IssueDate);

SELECT
    CASE Utility WHEN 'E' THEN 'Electricity' ELSE 'Water' END AS Utility,
    CAST(SUM(Cost) AS DECIMAL(12, 2)) AS BilledTotal,
    CAST(SUM(PaidTotal) AS DECIMAL(12, 2)) AS PreviouslyPaid,
    CAST(SUM(Cost) - SUM(PaidTotal) AS DECIMAL(12, 2)) AS BalanceImpact
FROM @Bills
GROUP BY Utility;
