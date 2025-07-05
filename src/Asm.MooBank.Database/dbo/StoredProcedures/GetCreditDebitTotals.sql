CREATE PROCEDURE dbo.GetCreditDebitTotals
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @StartDate = GREATEST(@StartDate, (SELECT Min(TransactionTime) FROM [Transaction] WHERE AccountId = @AccountId))
    SELECT @EndDate = LEAST(@EndDate, CAST(GETDATE() as DATE));

    -- Use TransactionSplitNetAmounts view to aggregate per transaction
    WITH SplitNet AS (
        SELECT TransactionId, NetAmount
        FROM dbo.TransactionSplitNetAmounts
    )
    SELECT
        t.TransactionTypeId AS TransactionType,
        SUM(CASE WHEN t.TransactionTypeId = 2 THEN -sn.NetAmount ELSE sn.NetAmount END) AS Total
    FROM dbo.[Transaction] t
    JOIN SplitNet sn ON sn.TransactionId = t.TransactionId
    WHERE t.AccountId = @AccountId
      AND t.TransactionTime >= @StartDate AND t.TransactionTime <= @EndDate
      AND t.ExcludeFromReporting = 0
    GROUP BY t.TransactionTypeId;
END
