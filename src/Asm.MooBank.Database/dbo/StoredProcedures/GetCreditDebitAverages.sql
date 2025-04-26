CREATE PROCEDURE dbo.GetCreditDebitAverages
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date,
    @Period varchar(7) = 'Monthly' -- or 'Yearly'
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate period span
    DECLARE @PeriodCount int = GREATEST(
        CASE
            WHEN @Period = 'Yearly' THEN DATEDIFF(YEAR, @StartDate, DATEADD(DAY, 1, @EndDate))
            ELSE DATEDIFF(MONTH, @StartDate, DATEADD(DAY, 1, @EndDate))
        END, 1);

    -- Use TransactionSplitNetAmounts view to aggregate per transaction
    WITH SplitNet AS (
        SELECT TransactionId, NetAmount
        FROM dbo.TransactionSplitNetAmounts
    ),
    Aggregated AS (
        SELECT
            t.TransactionTypeId AS TransactionType,
            SUM(CASE WHEN t.TransactionTypeId = 2 THEN -sn.NetAmount ELSE sn.NetAmount END) AS Total
        FROM dbo.[Transaction] t
        JOIN SplitNet sn ON sn.TransactionId = t.TransactionId
        WHERE t.AccountId = @AccountId
          AND t.TransactionTime >= @StartDate AND t.TransactionTime <= @EndDate
          AND t.ExcludeFromReporting = 0
        GROUP BY t.TransactionTypeId
    )
    SELECT
        a.TransactionType,
        CAST(a.Total / NULLIF(@PeriodCount, 0) AS DECIMAL(18, 4)) AS Average
    FROM Aggregated a;
END
