/*
    Demo account top-up, part 1 of 3. Run after new transactions land on the checking account.

    DemoMortgage.sql builds the ledger from the repayments checking held at the time, and then
    refuses to run again. When checking gains later repayments -- an import filling a gap, or the
    monthly job -- the mortgage stops where it was and the two drift apart.

    This adds a ledger entry for every checking 'Mortgage' payment that has none, continuing the
    amortisation from the balance the account currently owes rather than restarting it. Run it as
    often as you like: with nothing new on checking it writes nothing.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';
DECLARE @MonthlyRate DECIMAL(18, 9) = 0.055 / 12.0;      -- matches DemoMortgage.sql

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

IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE AccountId = @MortgageId)
    THROW 50000, 'The demo mortgage is empty. Run DemoMortgage.sql first.', 1;

/*
    A checking repayment is already accounted for when the mortgage carries a repayment on the same
    date. Matching on the date rather than on an identifier is what makes this re-runnable: nothing
    links the two ledgers, and a mortgage is paid at most once a day.
*/
DECLARE @Missing TABLE (
    n INT NOT NULL PRIMARY KEY,
    TxDate DATETIME2 NOT NULL,
    Repayment DECIMAL(12, 4) NOT NULL
);

INSERT INTO @Missing (n, TxDate, Repayment)
SELECT
    ROW_NUMBER() OVER (ORDER BY t.TransactionTime, t.TransactionId),
    t.TransactionTime,
    ABS(t.Amount)
FROM dbo.[Transaction] t
WHERE t.AccountId = @CheckingId
  AND EXISTS (
    SELECT 1
    FROM dbo.TransactionSplit ts
    INNER JOIN dbo.TransactionSplitTag tst ON tst.TransactionSplitId = ts.Id
    INNER JOIN dbo.Tag tg ON tg.Id = tst.TagId
    WHERE ts.TransactionId = t.TransactionId AND tg.[Name] = N'Mortgage' AND tg.FamilyId = @FamilyId)
  AND NOT EXISTS (
    SELECT 1
    FROM dbo.[Transaction] m
    WHERE m.AccountId = @MortgageId
      AND m.[Description] = 'Loan Repayment'
      AND CAST(m.TransactionTime AS DATE) = CAST(t.TransactionTime AS DATE));

IF NOT EXISTS (SELECT 1 FROM @Missing)
BEGIN
    PRINT 'The demo mortgage already covers every repayment on the checking account. Nothing to do.';
    RETURN;
END

/*
    The balance the account owes today is, by construction, the balance owing before the next
    repayment: every earlier repayment has already been applied to it. So the schedule continues
    from there instead of being recomputed from the original principal, and a top-up cannot
    disagree with the rows already written.
*/
DECLARE @Owing DECIMAL(18, 6) = (SELECT Balance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @MortgageId);

DECLARE @Schedule TABLE (
    n INT NOT NULL PRIMARY KEY,
    TxDate DATETIME2 NOT NULL,
    Interest DECIMAL(12, 4) NOT NULL,
    Principal DECIMAL(12, 4) NOT NULL,
    RepaymentTxId UNIQUEIDENTIFIER NOT NULL,
    InterestTxId UNIQUEIDENTIFIER NOT NULL,
    InterestSplitId UNIQUEIDENTIFIER NOT NULL,
    PrincipalSplitId UNIQUEIDENTIFIER NOT NULL,
    InterestTxSplitId UNIQUEIDENTIFIER NOT NULL
);

/*
    Each row's interest depends on the balance the row before it left, so this is recursive rather
    than closed form. Every column of the recursive member is cast to the anchor's type: arithmetic
    on DECIMAL widens the result, and a recursive CTE whose column types differ will not compile.

    Interest is rounded to the cent and principal takes the remainder, so the two always add back to
    the repayment exactly.
*/
;WITH Amortised AS (
    SELECT
        m.n,
        m.TxDate,
        m.Repayment,
        CAST(@Owing AS DECIMAL(18, 6)) AS BalanceBefore,
        CAST(ROUND(@Owing * @MonthlyRate, 2) AS DECIMAL(12, 4)) AS Interest
    FROM @Missing m
    WHERE m.n = 1
    UNION ALL
    SELECT
        n.n,
        n.TxDate,
        n.Repayment,
        CAST(a.BalanceBefore + a.Interest - a.Repayment AS DECIMAL(18, 6)),
        CAST(ROUND((a.BalanceBefore + a.Interest - a.Repayment) * @MonthlyRate, 2) AS DECIMAL(12, 4))
    FROM Amortised a
    INNER JOIN @Missing n ON n.n = a.n + 1
)
INSERT INTO @Schedule (n, TxDate, Interest, Principal, RepaymentTxId, InterestTxId, InterestSplitId, PrincipalSplitId, InterestTxSplitId)
SELECT
    a.n,
    a.TxDate,
    a.Interest,
    a.Repayment - a.Interest,
    NEWID(), NEWID(), NEWID(), NEWID(), NEWID()
FROM Amortised a
OPTION (MAXRECURSION 1000);

BEGIN TRAN;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.InterestTxId, @MortgageId, 1, s.Interest, 'Interest Charged', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.RepaymentTxId, @MortgageId, 2, 2, -(s.Interest + s.Principal), 'Loan Repayment', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

-- Split amounts are positive magnitudes even on a debit, which is how the application writes them.
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
    COUNT(*) AS RepaymentsAdded,
    MIN(TxDate) AS FirstAdded,
    MAX(TxDate) AS LastAdded,
    SUM(Interest) AS InterestAdded,
    SUM(Principal) AS PrincipalAdded
FROM @Schedule;

SELECT Balance AS AccountBalance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @MortgageId;
