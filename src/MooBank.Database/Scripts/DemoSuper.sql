/*
    Demo account backfill, part 3 of 6. Run DemoTagPurposes.sql first.

    Super never touches the bank account, so this is the one ledger generated rather than derived --
    but it is generated from the salary the checking account already holds, so the contributions
    line up with the pay dates a viewer can see.

      - An employer contribution on every salary date from the opening balance onward, at the
        superannuation guarantee rate in force for that date, tagged 'Employer Contribution'.
      - Earnings each quarter at a nominal 7% a year, scattered by up to three percentage points
        either way so the balance chart rises with some wobble rather than as a straight line.

    Salary dates before the opening balance are skipped: contributing against them would produce a
    balance that contradicts the figure the account opens with.

    No personal contributions are generated. The tag purpose is configured either way, so the report
    renders the series; an empty personal series beside a populated employer one is honest for this
    household.

    Idempotent: no-op if the super account holds anything beyond its opening balance.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

/*
    The salary on checking is net of tax; the guarantee is levied on gross. The generator that
    produced this history pays 7,000 net a month against a stated 110,000 gross a year, so gross is
    recovered by scaling the net figure rather than by inventing a separate salary series.
*/
DECLARE @GrossUp DECIMAL(9, 6) = 1.309524;      -- (110000 / 12) / 7000
DECLARE @NominalReturn FLOAT = 0.07;

DECLARE @CheckingId UNIQUEIDENTIFIER, @SuperId UNIQUEIDENTIFIER, @EmployerTagId INT;
DECLARE @OpeningDate DATE, @OpeningBalance DECIMAL(18, 6);

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

IF (SELECT COUNT(*) FROM dbo.[Transaction] WHERE AccountId = @SuperId) > 1
BEGIN
    PRINT 'Demo super already populated. Nothing to do.';
    RETURN;
END

SELECT @OpeningDate = CAST(TransactionTime AS DATE), @OpeningBalance = ABS(Amount)
FROM dbo.[Transaction] WHERE AccountId = @SuperId;

IF @OpeningDate IS NULL THROW 50000, 'Demo super account has no opening balance to build on.', 1;

-- Superannuation guarantee rates by financial year.
DECLARE @GuaranteeRates TABLE (ValidFrom DATE NOT NULL PRIMARY KEY, Rate DECIMAL(6, 4) NOT NULL);

INSERT INTO @GuaranteeRates (ValidFrom, Rate)
VALUES ('2013-07-01', 0.0925), ('2014-07-01', 0.0950), ('2021-07-01', 0.1000),
       ('2022-07-01', 0.1050), ('2023-07-01', 0.1100), ('2024-07-01', 0.1150),
       ('2025-07-01', 0.1200);

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
  AND CAST(s.TransactionTime AS DATE) >= @OpeningDate
  AND EXISTS (
    SELECT 1
    FROM dbo.TransactionSplit ts
    INNER JOIN dbo.TransactionSplitTag tst ON tst.TransactionSplitId = ts.Id
    INNER JOIN dbo.Tag tg ON tg.Id = tst.TagId
    WHERE ts.TransactionId = s.TransactionId AND tg.[Name] = N'Salary' AND tg.FamilyId = @FamilyId);

IF NOT EXISTS (SELECT 1 FROM @Contributions)
    THROW 50000, 'No salary-tagged transactions found on the demo checking account after the super opening balance.', 1;

/*
    Quarters from the first quarter-end after the account opens to the last one that has finished.
    The scatter on the return is derived arithmetically from the quarter itself, so re-running the
    script produces identical figures rather than a different history each time.
*/
DECLARE @Quarters TABLE (
    q INT NOT NULL PRIMARY KEY,
    QuarterEnd DATE NOT NULL,
    Contributions DECIMAL(18, 6) NOT NULL,
    Rate FLOAT NOT NULL,
    TxId UNIQUEIDENTIFIER NOT NULL,
    SplitId UNIQUEIDENTIFIER NOT NULL
);

DECLARE @FirstQuarterEnd DATE = (
    SELECT MIN(qe) FROM (VALUES
        (DATEFROMPARTS(YEAR(@OpeningDate), 3, 31)), (DATEFROMPARTS(YEAR(@OpeningDate), 6, 30)),
        (DATEFROMPARTS(YEAR(@OpeningDate), 9, 30)), (DATEFROMPARTS(YEAR(@OpeningDate), 12, 31))
    ) v(qe) WHERE qe >= @OpeningDate);

DECLARE @LastQuarterEnd DATE = (
    SELECT MAX(qe) FROM (VALUES
        (DATEFROMPARTS(YEAR(GETDATE()), 3, 31)), (DATEFROMPARTS(YEAR(GETDATE()), 6, 30)),
        (DATEFROMPARTS(YEAR(GETDATE()), 9, 30)), (DATEFROMPARTS(YEAR(GETDATE()), 12, 31)),
        (DATEFROMPARTS(YEAR(GETDATE()) - 1, 12, 31))
    ) v(qe) WHERE qe < CAST(GETDATE() AS DATE));

;WITH QuarterEnds AS (
    SELECT @FirstQuarterEnd AS QuarterEnd, 1 AS q
    UNION ALL
    SELECT EOMONTH(DATEADD(MONTH, 3, QuarterEnd)), q + 1
    FROM QuarterEnds
    WHERE EOMONTH(DATEADD(MONTH, 3, QuarterEnd)) <= @LastQuarterEnd
)
INSERT INTO @Quarters (q, QuarterEnd, Contributions, Rate, TxId, SplitId)
SELECT
    qe.q,
    qe.QuarterEnd,
    ISNULL((SELECT SUM(c.Amount) FROM @Contributions c
            WHERE CAST(c.TxDate AS DATE) <= qe.QuarterEnd
              AND CAST(c.TxDate AS DATE) > DATEADD(MONTH, -3, qe.QuarterEnd)), 0),
    @NominalReturn + ((((YEAR(qe.QuarterEnd) * 12 + MONTH(qe.QuarterEnd)) * 7919) % 601) - 300) / 10000.0,
    NEWID(),
    NEWID()
FROM QuarterEnds qe
OPTION (MAXRECURSION 400);

DECLARE @Earnings TABLE (
    TxId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SplitId UNIQUEIDENTIFIER NOT NULL,
    QuarterEnd DATE NOT NULL,
    Amount DECIMAL(12, 4) NOT NULL
);

/*
    Earnings compound, so each quarter's balance depends on the one before it. Contributions are
    weighted at half a quarter, on the basis that they arrive spread through it rather than on the
    first day.
*/
;WITH Balances AS (
    SELECT
        q.q, q.QuarterEnd, q.Contributions, q.Rate, q.TxId, q.SplitId,
        CAST(@OpeningBalance AS DECIMAL(18, 6)) AS BalanceBefore,
        CAST(ROUND((@OpeningBalance + q.Contributions / 2.0) * q.Rate / 4.0, 2) AS DECIMAL(18, 6)) AS Earnings
    FROM @Quarters q
    WHERE q.q = 1
    UNION ALL
    SELECT
        n.q, n.QuarterEnd, n.Contributions, n.Rate, n.TxId, n.SplitId,
        b.BalanceBefore + b.Contributions + b.Earnings,
        CAST(ROUND(((b.BalanceBefore + b.Contributions + b.Earnings) + n.Contributions / 2.0) * n.Rate / 4.0, 2) AS DECIMAL(18, 6))
    FROM Balances b
    INNER JOIN @Quarters n ON n.q = b.q + 1
)
INSERT INTO @Earnings (TxId, SplitId, QuarterEnd, Amount)
SELECT b.TxId, b.SplitId, b.QuarterEnd, b.Earnings
FROM Balances b
OPTION (MAXRECURSION 400);

BEGIN TRAN;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT c.TxId, @SuperId, 1, c.Amount, 'Employer Contribution', c.TxDate, 'Demo Backfill'
FROM @Contributions c;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT e.TxId, @SuperId, 1, e.Amount, 'Investment Earnings', CAST(e.QuarterEnd AS DATETIME2), 'Demo Backfill'
FROM @Earnings e;

INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
SELECT c.SplitId, c.TxId, c.Amount FROM @Contributions c
UNION ALL
SELECT e.SplitId, e.TxId, e.Amount FROM @Earnings e;

INSERT INTO dbo.TransactionSplitTag (TransactionSplitId, TagId)
SELECT c.SplitId, @EmployerTagId FROM @Contributions c;

COMMIT;

SELECT
    (SELECT COUNT(*) FROM @Contributions) AS Contributions,
    (SELECT SUM(Amount) FROM @Contributions) AS ContributionTotal,
    (SELECT COUNT(*) FROM @Earnings) AS EarningsRows,
    (SELECT SUM(Amount) FROM @Earnings) AS EarningsTotal,
    @OpeningBalance AS OpeningBalance;

SELECT Balance AS AccountBalance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @SuperId;
