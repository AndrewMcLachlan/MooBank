CREATE PROCEDURE dbo.GetMonthlyCreditDebitTotalsForAccounts
    @AccountIds dbo.GuidList READONLY,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @EndDate = LEAST(@EndDate, CAST(GETDATE() as DATE));

    -- Use TransactionSplitNetAmounts view to aggregate per transaction
    WITH SplitNet AS (
        SELECT TransactionId, NetAmount
        FROM dbo.TransactionSplitNetAmounts
    )
    SELECT
        t.AccountId,
        DATEFROMPARTS(YEAR(t.TransactionTime), MONTH(t.TransactionTime), 1) AS [Month],
        t.TransactionTypeId AS TransactionType,
        SUM(CASE WHEN t.TransactionTypeId = 2 THEN -CAST(sn.NetAmount AS DECIMAL(12,4)) ELSE CAST(sn.NetAmount AS DECIMAL(12,4)) END) AS Total
    FROM dbo.[Transaction] t
    JOIN @AccountIds a ON a.Id = t.AccountId
    JOIN SplitNet sn ON sn.TransactionId = t.TransactionId
    WHERE t.TransactionTime >= @StartDate AND t.TransactionTime < DATEADD(day, 1, @EndDate)
      AND t.ExcludeFromReporting = 0
    GROUP BY t.AccountId, DATEFROMPARTS(YEAR(t.TransactionTime), MONTH(t.TransactionTime), 1), t.TransactionTypeId;
END
