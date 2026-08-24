/*
    Demo account backfill, part 2 of 6. Run DemoTagPurposes.sql first.

    Builds the mortgage ledger from the repayments the checking account already holds, so the two
    cannot disagree. For each tagged 'Mortgage' payment on checking this writes, on the same date:

      - an interest charge, credited to the loan (a loan balance grows by the interest it accrues);
      - the repayment itself, debited from the loan, split into the interest and principal portions
        for that period, with the interest split tagged 'Mortgage Interest'.

    The balance runs positive-owing: 387,500 at the start, falling to roughly 298,000 today, which
    reads as a thirty-year loan twelve years in. That direction is forced by the reports. Principal
    vs Interest derives principal as (monthly debit total - interest-tagged splits), so the whole
    repayment has to be a debit on this account; the balance view subtracts every debit, so the
    interest has to be credited back for the balance to amortise rather than run away.

    Idempotent: no-op if the mortgage account holds anything beyond its opening balance.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

-- Loan terms. A 2,200 monthly repayment services a 387,500 principal over thirty years at 5.5%.
DECLARE @Principal DECIMAL(18, 6) = 387500.0;
DECLARE @AnnualRate DECIMAL(18, 9) = 0.055;
DECLARE @MonthlyRate FLOAT = 0.055 / 12.0;
DECLARE @Repayment DECIMAL(18, 6) = 2200.0;
DECLARE @OpeningDate DATETIME2 = '2014-01-01T00:00:00';

DECLARE @CheckingId UNIQUEIDENTIFIER, @MortgageId UNIQUEIDENTIFIER, @InterestTagId INT;

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

SELECT @MortgageId = f.Id
FROM (
    SELECT i.[Name], MIN(i.Id) AS Id, COUNT(*) AS Matches
    FROM dbo.Instrument i
    WHERE EXISTS (
        SELECT 1 FROM dbo.InstrumentOwner io
        INNER JOIN dbo.[User] u ON u.Id = io.UserId
        WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
    GROUP BY i.[Name]
) f WHERE f.[Name] = N'Mortgage' AND f.Matches = 1;

IF @CheckingId IS NULL THROW 50000, 'Demo checking account not found, or matched more than once.', 1;
IF @MortgageId IS NULL THROW 50000, 'Demo mortgage account not found, or matched more than once.', 1;

SELECT @InterestTagId = TagId FROM dbo.AccountTagPurpose WHERE InstrumentId = @MortgageId AND Purpose = 4;

IF @InterestTagId IS NULL THROW 50000, 'No MortgageInterest tag purpose on the demo mortgage. Run DemoTagPurposes.sql first.', 1;

IF (SELECT COUNT(*) FROM dbo.[Transaction] WHERE AccountId = @MortgageId) > 1
BEGIN
    PRINT 'Demo mortgage already populated. Nothing to do.';
    RETURN;
END

DECLARE @Schedule TABLE (
    n INT NOT NULL PRIMARY KEY,
    TxDate DATETIME2 NOT NULL,
    Repayment DECIMAL(12, 4) NOT NULL,
    Interest DECIMAL(12, 4) NOT NULL,
    Principal DECIMAL(12, 4) NOT NULL,
    RepaymentTxId UNIQUEIDENTIFIER NOT NULL,
    InterestTxId UNIQUEIDENTIFIER NOT NULL,
    InterestSplitId UNIQUEIDENTIFIER NOT NULL,
    PrincipalSplitId UNIQUEIDENTIFIER NOT NULL,
    InterestTxSplitId UNIQUEIDENTIFIER NOT NULL
);

/*
    Amortisation in closed form rather than row by row: the balance owing before payment n is
    B0*(1+r)^(n-1) - P*(((1+r)^(n-1) - 1)/r), so the whole schedule is one set-based pass instead of
    a cursor. Interest is rounded to the cent and principal takes the remainder, which keeps
    interest + principal exactly equal to the repayment on every row.
*/
;WITH Payments AS (
    SELECT
        t.TransactionTime,
        ABS(t.Amount) AS Repayment,
        ROW_NUMBER() OVER (ORDER BY t.TransactionTime, t.TransactionId) AS n
    FROM dbo.[Transaction] t
    WHERE t.AccountId = @CheckingId
      AND EXISTS (
        SELECT 1
        FROM dbo.TransactionSplit ts
        INNER JOIN dbo.TransactionSplitTag tst ON tst.TransactionSplitId = ts.Id
        INNER JOIN dbo.Tag tg ON tg.Id = tst.TagId
        WHERE ts.TransactionId = t.TransactionId AND tg.[Name] = N'Mortgage' AND tg.FamilyId = @FamilyId)
),
Balances AS (
    SELECT
        p.n,
        p.TransactionTime,
        p.Repayment,
        CAST(
            @Principal * POWER(CAST(1.0 + @MonthlyRate AS FLOAT), p.n - 1)
            - @Repayment * ((POWER(CAST(1.0 + @MonthlyRate AS FLOAT), p.n - 1) - 1.0) / @MonthlyRate)
        AS DECIMAL(18, 6)) AS BalanceBefore
    FROM Payments p
)
INSERT INTO @Schedule (n, TxDate, Repayment, Interest, Principal, RepaymentTxId, InterestTxId, InterestSplitId, PrincipalSplitId, InterestTxSplitId)
SELECT
    b.n,
    b.TransactionTime,
    b.Repayment,
    ROUND(b.BalanceBefore * @MonthlyRate, 2),
    b.Repayment - ROUND(b.BalanceBefore * @MonthlyRate, 2),
    NEWID(), NEWID(), NEWID(), NEWID(), NEWID()
FROM Balances b;

IF NOT EXISTS (SELECT 1 FROM @Schedule)
    THROW 50000, 'No mortgage-tagged transactions found on the demo checking account.', 1;

BEGIN TRAN;

-- The single existing row is re-dated and re-based: it sat at 2016-05-09, two years after the
-- repayments on checking begin, holding a figure that no longer relates to them.
UPDATE dbo.[Transaction]
SET TransactionTime = @OpeningDate,
    Amount = @Principal,
    TransactionTypeId = 1,          -- TransactionType.Credit
    TransactionSubTypeId = 1,       -- TransactionSubType.OpeningBalance
    [Description] = 'Opening Balance',
    [Source] = 'Demo Backfill'
WHERE AccountId = @MortgageId;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.InterestTxId, @MortgageId, 1, s.Interest, 'Interest Charged', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.RepaymentTxId, @MortgageId, 2, 2, -s.Repayment, 'Loan Repayment', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

/*
    Every transaction needs at least one split: the reporting procedures join transactions to
    splits, so an unsplit row is invisible to them.
*/
INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
SELECT s.InterestTxSplitId, s.InterestTxId, s.Interest FROM @Schedule s
UNION ALL
SELECT s.InterestSplitId, s.RepaymentTxId, s.Interest FROM @Schedule s
UNION ALL
SELECT s.PrincipalSplitId, s.RepaymentTxId, s.Principal FROM @Schedule s;

INSERT INTO dbo.TransactionSplitTag (TransactionSplitId, TagId)
SELECT s.InterestSplitId, @InterestTagId FROM @Schedule s;

COMMIT;

SELECT
    COUNT(*) AS Repayments,
    MIN(TxDate) AS FirstRepayment,
    MAX(TxDate) AS LastRepayment,
    SUM(Interest) AS TotalInterest,
    SUM(Principal) AS TotalPrincipal,
    @Principal + SUM(Interest) - SUM(Repayment) AS ClosingBalance
FROM @Schedule;

SELECT Balance AS AccountBalance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @MortgageId;
