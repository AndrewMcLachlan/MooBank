/*
    Demo account top-up, part 2 of 3. Run after new transactions land on the checking account.

    DemoSuper.sql generates contributions from the salary dates checking held at the time, and then
    refuses to run again. This adds a contribution for every salary date that has none, and an
    earnings row for every finished quarter that has none.

    Run it as often as you like: with nothing new on checking it writes nothing.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

-- Both match DemoSuper.sql, so a topped-up account continues the series rather than stepping.
DECLARE @GrossUp DECIMAL(9, 6) = 1.309524;
DECLARE @NominalReturn FLOAT = 0.07;

DECLARE @CheckingId UNIQUEIDENTIFIER, @SuperId UNIQUEIDENTIFIER, @EmployerTagId INT;

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

SELECT @SuperId = f.Id
FROM (
    SELECT i.[Name], MIN(i.Id) AS Id, COUNT(*) AS Matches
    FROM dbo.Instrument i
    WHERE EXISTS (
        SELECT 1 FROM dbo.InstrumentOwner io
        INNER JOIN dbo.[User] u ON u.Id = io.UserId
        WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
    GROUP BY i.[Name]
) f WHERE f.[Name] = N'Super' AND f.Matches = 1;

IF @CheckingId IS NULL THROW 50000, 'Demo checking account not found, or matched more than once.', 1;
IF @SuperId IS NULL THROW 50000, 'Demo super account not found, or matched more than once.', 1;

SELECT @EmployerTagId = TagId FROM dbo.AccountTagPurpose WHERE InstrumentId = @SuperId AND Purpose = 2;

IF @EmployerTagId IS NULL THROW 50000, 'No EmployerContribution tag purpose on the demo super account. Run DemoTagPurposes.sql first.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE AccountId = @SuperId AND [Description] = 'Employer Contribution')
    THROW 50000, 'The demo super account has no contributions. Run DemoSuper.sql first.', 1;

DECLARE @GuaranteeRates TABLE (ValidFrom DATE NOT NULL PRIMARY KEY, Rate DECIMAL(6, 4) NOT NULL);

INSERT INTO @GuaranteeRates (ValidFrom, Rate)
VALUES ('2013-07-01', 0.0925), ('2014-07-01', 0.0950), ('2021-07-01', 0.1000),
       ('2022-07-01', 0.1050), ('2023-07-01', 0.1100), ('2024-07-01', 0.1150),
       ('2025-07-01', 0.1200);

/*
    A salary date is already accounted for when the super account carries a contribution on the same
    date. Matching on the date is what makes this re-runnable: nothing links the two ledgers.
*/
DECLARE @Contributions TABLE (
    TxId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SplitId UNIQUEIDENTIFIER NOT NULL,
    TxDate DATETIME2 NOT NULL,
    Amount DECIMAL(12, 4) NOT NULL
);

INSERT INTO @Contributions (TxId, SplitId, TxDate, Amount)
SELECT NEWID(), NEWID(), s.TransactionTime, ROUND(ABS(s.Amount) * @GrossUp * r.Rate, 2)
FROM dbo.[Transaction] s
CROSS APPLY (
    SELECT TOP 1 g.Rate
    FROM @GuaranteeRates g
    WHERE g.ValidFrom <= CAST(s.TransactionTime AS DATE)
    ORDER BY g.ValidFrom DESC
) r
WHERE s.AccountId = @CheckingId
  AND EXISTS (
    SELECT 1
    FROM dbo.TransactionSplit ts
    INNER JOIN dbo.TransactionSplitTag tst ON tst.TransactionSplitId = ts.Id
    INNER JOIN dbo.Tag tg ON tg.Id = tst.TagId
    WHERE ts.TransactionId = s.TransactionId AND tg.[Name] = N'Salary' AND tg.FamilyId = @FamilyId)
  AND NOT EXISTS (
    SELECT 1
    FROM dbo.[Transaction] c
    WHERE c.AccountId = @SuperId
      AND c.[Description] = 'Employer Contribution'
      AND CAST(c.TransactionTime AS DATE) = CAST(s.TransactionTime AS DATE));

BEGIN TRAN;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT c.TxId, @SuperId, 1, c.Amount, 'Employer Contribution', c.TxDate, 'Demo Backfill'
FROM @Contributions c;

INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
SELECT c.SplitId, c.TxId, c.Amount FROM @Contributions c;

INSERT INTO dbo.TransactionSplitTag (TransactionSplitId, TagId)
SELECT c.SplitId, @EmployerTagId FROM @Contributions c;

/*
    Earnings for any finished quarter that has none. The contributions above are already committed
    to the table at this point, so the balances below include them.
*/
DECLARE @Quarters TABLE (
    q INT NOT NULL PRIMARY KEY,
    QuarterEnd DATE NOT NULL,
    Contributions DECIMAL(18, 6) NOT NULL,
    BalanceAtEnd DECIMAL(18, 6) NOT NULL,
    Rate FLOAT NOT NULL,
    TxId UNIQUEIDENTIFIER NOT NULL,
    SplitId UNIQUEIDENTIFIER NOT NULL
);

INSERT INTO @Quarters (q, QuarterEnd, Contributions, BalanceAtEnd, Rate, TxId, SplitId)
SELECT
    ROW_NUMBER() OVER (ORDER BY qe.QuarterEnd),
    qe.QuarterEnd,
    ISNULL((SELECT SUM(t.Amount) FROM dbo.[Transaction] t
            WHERE t.AccountId = @SuperId AND t.[Description] = 'Employer Contribution'
              AND CAST(t.TransactionTime AS DATE) <= qe.QuarterEnd
              AND CAST(t.TransactionTime AS DATE) > DATEADD(MONTH, -3, qe.QuarterEnd)), 0),
    -- The balance the account reaches by that quarter end, before the earnings added below.
    ISNULL((SELECT SUM(CASE WHEN t.TransactionTypeId = 1 THEN t.Amount ELSE -ABS(t.Amount) END)
            FROM dbo.[Transaction] t
            WHERE t.AccountId = @SuperId AND CAST(t.TransactionTime AS DATE) <= qe.QuarterEnd), 0),
    -- Scatter derived from the quarter itself, so a re-run produces the same figure.
    @NominalReturn + ((((YEAR(qe.QuarterEnd) * 12 + MONTH(qe.QuarterEnd)) * 7919) % 601) - 300) / 10000.0,
    NEWID(),
    NEWID()
FROM (
    -- The months are already the quarter-end ones, so EOMONTH is applied to them directly.
    SELECT DISTINCT EOMONTH(v.d) AS QuarterEnd
    FROM (
        SELECT DATEADD(MONTH, n.n, CAST('2014-01-31' AS DATE)) AS d
        FROM (SELECT TOP 300 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
              FROM sys.all_objects) n
    ) v
    WHERE MONTH(v.d) IN (3, 6, 9, 12)
) qe
WHERE qe.QuarterEnd < CAST(GETDATE() AS DATE)
  AND qe.QuarterEnd >= (SELECT MIN(CAST(TransactionTime AS DATE)) FROM dbo.[Transaction] WHERE AccountId = @SuperId)
  AND NOT EXISTS (
    SELECT 1 FROM dbo.[Transaction] e
    WHERE e.AccountId = @SuperId
      AND e.[Description] = 'Investment Earnings'
      AND CAST(e.TransactionTime AS DATE) = qe.QuarterEnd);

DECLARE @Earnings TABLE (
    TxId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SplitId UNIQUEIDENTIFIER NOT NULL,
    QuarterEnd DATE NOT NULL,
    Amount DECIMAL(12, 4) NOT NULL
);

/*
    Earnings compound, so each quarter's credit sits on top of the ones before it. BalanceAtEnd
    already carries every transaction to that date; the running total adds the earnings this script
    is about to write. Contributions are weighted at half a quarter, on the basis that they arrive
    spread through it.
*/
;WITH Compounded AS (
    SELECT
        q.q, q.QuarterEnd, q.TxId, q.SplitId,
        CAST(ROUND((q.BalanceAtEnd - q.Contributions / 2.0) * q.Rate / 4.0, 2) AS DECIMAL(12, 4)) AS Earnings,
        CAST(ROUND((q.BalanceAtEnd - q.Contributions / 2.0) * q.Rate / 4.0, 2) AS DECIMAL(18, 6)) AS Accumulated
    FROM @Quarters q
    WHERE q.q = 1
    UNION ALL
    SELECT
        n.q, n.QuarterEnd, n.TxId, n.SplitId,
        CAST(ROUND((n.BalanceAtEnd + c.Accumulated - n.Contributions / 2.0) * n.Rate / 4.0, 2) AS DECIMAL(12, 4)),
        CAST(c.Accumulated + ROUND((n.BalanceAtEnd + c.Accumulated - n.Contributions / 2.0) * n.Rate / 4.0, 2) AS DECIMAL(18, 6))
    FROM Compounded c
    INNER JOIN @Quarters n ON n.q = c.q + 1
)
INSERT INTO @Earnings (TxId, SplitId, QuarterEnd, Amount)
SELECT c.TxId, c.SplitId, c.QuarterEnd, c.Earnings
FROM Compounded c
WHERE c.Earnings > 0
OPTION (MAXRECURSION 1000);

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT e.TxId, @SuperId, 1, e.Amount, 'Investment Earnings', CAST(e.QuarterEnd AS DATETIME2), 'Demo Backfill'
FROM @Earnings e;

INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
SELECT e.SplitId, e.TxId, e.Amount FROM @Earnings e;

COMMIT;

SELECT
    (SELECT COUNT(*) FROM @Contributions) AS ContributionsAdded,
    (SELECT ISNULL(SUM(Amount), 0) FROM @Contributions) AS ContributionTotal,
    (SELECT COUNT(*) FROM @Earnings) AS EarningsAdded,
    (SELECT ISNULL(SUM(Amount), 0) FROM @Earnings) AS EarningsTotal;

SELECT Balance AS AccountBalance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @SuperId;
