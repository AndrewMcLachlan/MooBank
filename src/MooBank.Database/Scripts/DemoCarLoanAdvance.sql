/*
    Demo account repair. Run once, after DemoCarLoan.sql.

    DemoCarLoan.sql recorded the loan on both sides but only one of them in cash: the loan account
    opens owing 35,000, and the repayments leave the checking account, but the 35,000 the borrower
    actually received was never paid in. The checking account has therefore been funding a car it
    never got the money for, and is out by the repayments made to date -- 34,366.15 over 49 months,
    which is most of the deficit showing on the dashboard.

    This credits the advance to checking on the drawdown date. The account then bears only the cost
    of the loan rather than its whole principal: 35,000 in against 42,081.00 paid out over the full
    sixty months, a difference of 7,081.00, which is the interest.

    The credit is marked ExcludeFromReporting: it is financing, not income, and a one-off 35,000
    would otherwise dominate every income and cash-flow chart the demo is meant to show off. The
    balance view ignores that flag, so the account balance still reflects it.

    Idempotent: no-op if the advance is already there.

    Run by hand against production. Not referenced by the .sqlproj.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FamilyId UNIQUEIDENTIFIER = 'B0DDD93D-827F-4716-B4E2-D1922FAF7E27';

-- Both match DemoCarLoan.sql.
DECLARE @Advance DECIMAL(12, 4) = 35000.0;
DECLARE @DrawnOn DATE = '2022-07-01';

DECLARE @CheckingId UNIQUEIDENTIFIER, @LoanId UNIQUEIDENTIFIER;
DECLARE @TxId UNIQUEIDENTIFIER = NEWID(), @SplitId UNIQUEIDENTIFIER = NEWID();

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

SELECT @LoanId = f.Id
FROM (
    SELECT i.[Name], MIN(i.Id) AS Id, COUNT(*) AS Matches
    FROM dbo.Instrument i
    WHERE EXISTS (
        SELECT 1 FROM dbo.InstrumentOwner io
        INNER JOIN dbo.[User] u ON u.Id = io.UserId
        WHERE io.InstrumentId = i.Id AND u.FamilyId = @FamilyId)
    GROUP BY i.[Name]
) f WHERE f.[Name] = N'Car Loan' AND f.Matches = 1;

IF @CheckingId IS NULL THROW 50000, 'Demo checking account not found, or matched more than once.', 1;
IF @LoanId IS NULL THROW 50000, 'Demo car loan not found. Run DemoCarLoan.sql first.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.[Transaction]
    WHERE AccountId = @CheckingId
      AND [Description] = 'Car Loan Advance'
      AND CAST(TransactionTime AS DATE) = @DrawnOn)
BEGIN
    PRINT 'The car loan advance is already on the demo checking account. Nothing to do.';
    RETURN;
END

BEGIN TRAN;

INSERT INTO dbo.[Transaction] (TransactionId, AccountId, TransactionTypeId, TransactionSubTypeId, Amount, [Description], TransactionTime, ExcludeFromReporting, [Source])
VALUES (@TxId, @CheckingId, 1, 10, @Advance, 'Car Loan Advance', CAST(@DrawnOn AS DATETIME2), 1, 'Demo Backfill');

-- Every transaction needs a split: the reporting procedures join through them.
INSERT INTO dbo.TransactionSplit (Id, TransactionId, Amount)
VALUES (@SplitId, @TxId, @Advance);

COMMIT;

SELECT
    @Advance AS AdvanceCredited,
    (SELECT COUNT(*) FROM dbo.[Transaction]
     WHERE AccountId = @CheckingId AND [Description] LIKE '%CAR LOAN REPAYMENT%') AS RepaymentsToDate,
    (SELECT ISNULL(SUM(ABS(Amount)), 0) FROM dbo.[Transaction]
     WHERE AccountId = @CheckingId AND [Description] LIKE '%CAR LOAN REPAYMENT%') AS RepaidToDate;

SELECT Balance AS CheckingBalance FROM dbo.TransactionInstrumentBalance WHERE InstrumentId = @CheckingId;
