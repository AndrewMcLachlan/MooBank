/*
    Demo account backfill, part 6 of 6. Run DemoTagPurposes.sql first.

    The savings account stopped at 2025-11-30, months behind the checking account. The recurring job
    fills one month -- the previous one -- and never looks further back, so nothing else will ever
    close this gap.

    Extends savings from the month after its last transaction to the end of the last whole month:
    a monthly transfer in, sized from the account's own recent history rather than a figure invented
    here, and interest credited at each month end. Interest is tagged with the account's Interest
    purpose so the Savings Interest report covers the new months as well as the old ones.

    Idempotent: no-op when the account already reaches the end of the last whole month.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

DECLARE @AnnualRate FLOAT = 0.045;      -- matches the rate the existing history was generated at
DECLARE @TransferDay INT = 2;

DECLARE @SavingsId UNIQUEIDENTIFIER, @InterestTagId INT;
DECLARE @LastTransaction DATE, @OpeningBalance DECIMAL(18, 6), @MonthlyTransfer DECIMAL(12, 4);

SELECT @SavingsId = f.Id
FROM (
    SELECT i.[Name], MIN(i.Id) AS Id, COUNT(*) AS Matches
    FROM dbo.Instrument i
    WHERE EXISTS (
        SELECT 1 FROM dbo.InstrumentOwner io
        INNER JOIN dbo.[User] u ON u.Id = io.UserId
        WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
    GROUP BY i.[Name]
) f WHERE f.[Name] = N'Savings Account' AND f.Matches = 1;

IF @SavingsId IS NULL THROW 50000, 'Demo savings account not found, or matched more than once.', 1;

SELECT @InterestTagId = TagId FROM dbo.AccountTagPurpose WHERE InstrumentId = @SavingsId AND Purpose = 1;

IF @InterestTagId IS NULL THROW 50000, 'No Interest tag purpose on the demo savings account. Run DemoTagPurposes.sql first.', 1;

SELECT @LastTransaction = [LastTransaction], @OpeningBalance = Balance
FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @SavingsId;

IF @LastTransaction IS NULL THROW 50000, 'Demo savings account has no transactions to extend from.', 1;

-- The first month to fill, and the last month that has finished.
DECLARE @From DATE = DATEFROMPARTS(YEAR(DATEADD(MONTH, 1, @LastTransaction)), MONTH(DATEADD(MONTH, 1, @LastTransaction)), 1);
DECLARE @To DATE = EOMONTH(GETDATE(), -1);

IF @From > @To
BEGIN
    PRINT 'Demo savings already reaches the end of the last whole month. Nothing to do.';
    RETURN;
END

/*
    Sized from the account's own last year of deposits rather than a figure chosen here, so the new
    months continue the pattern the charts already show. Interest credits are excluded so the
    deposit average is not inflated by them.
*/
SELECT @MonthlyTransfer = ROUND(AVG(m.Total), 2)
FROM (
    SELECT SUM(t.Amount) AS Total
    FROM dbo.[Transaction] t
    WHERE t.AccountId = @SavingsId
      AND t.TransactionTypeId = 1
      AND t.[Description] NOT LIKE '%Interest%'
      AND t.TransactionTime >= DATEADD(MONTH, -12, @LastTransaction)
    GROUP BY DATEFROMPARTS(YEAR(t.TransactionTime), MONTH(t.TransactionTime), 1)
) m;

IF @MonthlyTransfer IS NULL OR @MonthlyTransfer <= 0
    THROW 50000, 'Could not determine a monthly transfer from the demo savings history.', 1;

DECLARE @Months TABLE (
    m INT NOT NULL PRIMARY KEY,
    MonthStart DATE NOT NULL,
    MonthEnd DATE NOT NULL,
    TransferDate DATETIME2 NOT NULL,
    Interest DECIMAL(12, 4) NOT NULL,
    TransferTxId UNIQUEIDENTIFIER NOT NULL,
    TransferSplitId UNIQUEIDENTIFIER NOT NULL,
    InterestTxId UNIQUEIDENTIFIER NOT NULL,
    InterestSplitId UNIQUEIDENTIFIER NOT NULL
);

/*
    Interest compounds, so each month's credit depends on the balance the month before it left
    behind. The deposit lands early in the month and so is treated as earning for the whole of it.
*/
;WITH Months AS (
    SELECT
        1 AS m,
        @From AS MonthStart,
        CAST(@OpeningBalance AS DECIMAL(18, 6)) AS BalanceBefore,
        CAST(ROUND((@OpeningBalance + @MonthlyTransfer) * @AnnualRate / 12.0, 2) AS DECIMAL(18, 6)) AS Interest
    UNION ALL
    SELECT
        p.m + 1,
        DATEADD(MONTH, 1, p.MonthStart),
        -- Cast, not decoration: adding a DECIMAL(18,6) to a DECIMAL(12,4) widens the result, and a
        -- recursive member whose column type differs from the anchor's will not compile.
        CAST(p.BalanceBefore + @MonthlyTransfer + p.Interest AS DECIMAL(18, 6)),
        CAST(ROUND((p.BalanceBefore + @MonthlyTransfer + p.Interest + @MonthlyTransfer) * @AnnualRate / 12.0, 2) AS DECIMAL(18, 6))
    FROM Months p
    WHERE DATEADD(MONTH, 1, p.MonthStart) <= @To
)
INSERT INTO @Months (m, MonthStart, MonthEnd, TransferDate, Interest, TransferTxId, TransferSplitId, InterestTxId, InterestSplitId)
SELECT
    mo.m,
    mo.MonthStart,
    EOMONTH(mo.MonthStart),
    CAST(DATEADD(DAY, @TransferDay - 1, mo.MonthStart) AS DATETIME2),
    mo.Interest,
    NEWID(), NEWID(), NEWID(), NEWID()
FROM Months mo
OPTION (MAXRECURSION 400);

BEGIN TRAN;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, [Source])
SELECT
    mo.TransferTxId, @SavingsId, 1, 10, @MonthlyTransfer,
    CONCAT('Transfer - Osko Payment to TRANSACTION ACCOUNT - Receipt ', 300000 + mo.m, '  - Ref SAV', 10000 + mo.m),
    mo.TransferDate, 'Demo Backfill'
FROM @Months mo;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT mo.InterestTxId, @SavingsId, 1, mo.Interest, 'Interest Credit', CAST(mo.MonthEnd AS DATETIME2), 'Demo Backfill'
FROM @Months mo;

INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
SELECT mo.TransferSplitId, mo.TransferTxId, @MonthlyTransfer FROM @Months mo
UNION ALL
SELECT mo.InterestSplitId, mo.InterestTxId, mo.Interest FROM @Months mo;

INSERT INTO dbo.TransactionSplitTag (TransactionSplitId, TagId)
SELECT mo.InterestSplitId, @InterestTagId FROM @Months mo;

COMMIT;

SELECT
    COUNT(*) AS MonthsFilled,
    MIN(MonthStart) AS FirstMonth,
    MAX(MonthEnd) AS LastMonth,
    @MonthlyTransfer AS MonthlyTransfer,
    SUM(Interest) AS TotalInterest
FROM @Months;

SELECT Balance AS AccountBalance, [LastTransaction] FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @SavingsId;
