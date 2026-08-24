/*
    Demo account backfill, part 5 of 6.

    A car loan is the one piece with no existing side, so this writes both halves: the repayments
    leaving the checking account and the loan ledger they service. It is deliberately bounded --
    35,000 drawn on 2022-07-01 over five years at 7.5%, repaying 701.35 a month, maturing mid-2027 --
    so the demo shows a loan in progress rather than one already closed.

    The loan ledger takes the same shape as the mortgage: the balance runs positive-owing, each
    repayment is a debit split into a tagged interest portion and principal, and the month's interest
    is credited back so the balance amortises. See DemoMortgage.sql for why that direction is forced.

    This is the only script that modifies the checking account, which is otherwise accurate and
    almost entirely tagged. It is separate from the others so it can be run, reviewed or skipped on
    its own.

    Idempotent: no-op if the demo family already owns a Car Loan account.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

DECLARE @Advance DECIMAL(18, 6) = 35000.0;
DECLARE @MonthlyRate FLOAT = 0.075 / 12.0;
DECLARE @Repayment DECIMAL(12, 4) = 701.35;
DECLARE @DrawnOn DATE = '2022-07-01';
DECLARE @Term INT = 60;

DECLARE @CheckingId UNIQUEIDENTIFIER, @OwnerId UNIQUEIDENTIFIER;
DECLARE @LoanId UNIQUEIDENTIFIER = NEWID();
DECLARE @CarLoanTagId INT, @InterestTagId INT;

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
    WHERE u.FamilyId = @FamilyId AND i.[Name] = N'Car Loan')
BEGIN
    PRINT 'Demo car loan already exists. Nothing to do.';
    RETURN;
END

/*
    Repayments run monthly from the month after drawdown, up to the last one that has fallen due.
    Interest is taken from the closed-form balance owing before each payment, and principal takes
    the remainder, so the two always add back to the repayment exactly.
*/
DECLARE @Schedule TABLE (
    n INT NOT NULL PRIMARY KEY,
    TxDate DATETIME2 NOT NULL,
    Interest DECIMAL(12, 4) NOT NULL,
    Principal DECIMAL(12, 4) NOT NULL,
    CheckingTxId UNIQUEIDENTIFIER NOT NULL,
    CheckingSplitId UNIQUEIDENTIFIER NOT NULL,
    RepaymentTxId UNIQUEIDENTIFIER NOT NULL,
    InterestTxId UNIQUEIDENTIFIER NOT NULL,
    InterestSplitId UNIQUEIDENTIFIER NOT NULL,
    PrincipalSplitId UNIQUEIDENTIFIER NOT NULL,
    InterestTxSplitId UNIQUEIDENTIFIER NOT NULL
);

;WITH Instalments AS (
    SELECT 1 AS n
    UNION ALL
    SELECT n + 1 FROM Instalments WHERE n < @Term
),
Due AS (
    SELECT i.n, DATEADD(MONTH, i.n, @DrawnOn) AS DueOn
    FROM Instalments i
),
Balances AS (
    SELECT
        d.n,
        d.DueOn,
        CAST(
            @Advance * POWER(CAST(1.0 + @MonthlyRate AS FLOAT), d.n - 1)
            - @Repayment * ((POWER(CAST(1.0 + @MonthlyRate AS FLOAT), d.n - 1) - 1.0) / @MonthlyRate)
        AS DECIMAL(18, 6)) AS BalanceBefore
    FROM Due d
    WHERE d.DueOn <= CAST(GETDATE() AS DATE)
)
INSERT INTO @Schedule (n, TxDate, Interest, Principal, CheckingTxId, CheckingSplitId, RepaymentTxId, InterestTxId, InterestSplitId, PrincipalSplitId, InterestTxSplitId)
SELECT
    b.n,
    CAST(b.DueOn AS DATETIME2),
    ROUND(b.BalanceBefore * @MonthlyRate, 2),
    @Repayment - ROUND(b.BalanceBefore * @MonthlyRate, 2),
    NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), NEWID(), NEWID()
FROM Balances b
OPTION (MAXRECURSION 200);

IF NOT EXISTS (SELECT 1 FROM @Schedule)
    THROW 50000, 'No car loan repayments fall due before today. Check @DrawnOn.', 1;

BEGIN TRAN;

-- 'Car Loan' tags the repayments on checking; 'Car Loan Interest' carries the interest purpose on
-- the loan itself. Both need a TagSettings row or the reports will not see them.
INSERT INTO dbo.Tag ([Name], FamilyId)
SELECT n.[Name], @FamilyId
FROM (VALUES (N'Car Loan'), (N'Car Loan Interest')) n([Name])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Tag t
    WHERE t.[Name] = n.[Name] AND t.FamilyId = @FamilyId AND t.Deleted = 0);

SELECT @CarLoanTagId = Id FROM dbo.Tag WHERE [Name] = N'Car Loan' AND FamilyId = @FamilyId AND Deleted = 0;
SELECT @InterestTagId = Id FROM dbo.Tag WHERE [Name] = N'Car Loan Interest' AND FamilyId = @FamilyId AND Deleted = 0;

INSERT INTO dbo.TagSettings (TagId)
SELECT v.TagId FROM (VALUES (@CarLoanTagId), (@InterestTagId)) v(TagId)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TagSettings s WHERE s.TagId = v.TagId);

INSERT INTO dbo.Instrument (Id, [Name], [Description], ShareWithFamily, Slug)
VALUES (@LoanId, N'Car Loan', N'Five year car loan', 1, 'demo-car-loan');

INSERT INTO dbo.TransactionInstrument (InstrumentId) VALUES (@LoanId);

INSERT INTO dbo.LogicalAccount (InstrumentId, AccountTypeId) VALUES (@LoanId, 7);   -- AccountType.Loan

INSERT INTO dbo.InstrumentOwner (InstrumentId, UserId) VALUES (@LoanId, @OwnerId);

INSERT INTO dbo.AccountTagPurpose (InstrumentId, Purpose, TagId)
VALUES (@LoanId, 4, @InterestTagId);      -- TagPurpose.MortgageInterest

INSERT INTO dbo.[Transaction] (AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, [Source])
VALUES (@LoanId, 1, 1, @Advance, 'Loan Advance', CAST(@DrawnOn AS DATETIME2), 'Demo Backfill');

-- The repayments leaving checking. These are the only rows this backfill adds to that account.
INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.CheckingTxId, @CheckingId, 2, 7, -@Repayment, 'Direct Debit - CAR LOAN REPAYMENT', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
-- Split amounts are positive magnitudes even on a debit, which is how the application writes them.
SELECT s.CheckingSplitId, s.CheckingTxId, @Repayment FROM @Schedule s;

INSERT INTO dbo.TransactionSplitTag (TransactionSplitId, TagId)
SELECT s.CheckingSplitId, @CarLoanTagId FROM @Schedule s;

-- The loan ledger.
INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.InterestTxId, @LoanId, 1, s.Interest, 'Interest Charged', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, [Source])
SELECT s.RepaymentTxId, @LoanId, 2, 2, -@Repayment, 'Loan Repayment', s.TxDate, 'Demo Backfill'
FROM @Schedule s;

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
    @Advance + SUM(Interest) - (COUNT(*) * @Repayment) AS ClosingBalance
FROM @Schedule;

SELECT Balance AS LoanBalance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @LoanId;
