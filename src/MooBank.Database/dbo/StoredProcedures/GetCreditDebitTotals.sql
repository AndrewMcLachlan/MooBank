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
    -- A split whose tags are all excluded from reporting contributes nothing, so a
    -- transfer leaves the totals instead of inflating both sides. One reportable tag keeps
    -- the whole split in. A tag with no settings row counts as reportable.
    WITH SplitNet AS (
        SELECT sn.TransactionId, sn.NetAmount
        FROM dbo.TransactionSplitNetAmounts sn
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.TransactionSplitTag tst
            JOIN dbo.TagSettings ts ON ts.TagId = tst.TagId
            WHERE tst.TransactionSplitId = sn.Id
              AND ts.ExcludeFromReporting = 1
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.TransactionSplitTag tst
            LEFT JOIN dbo.TagSettings ts ON ts.TagId = tst.TagId
            WHERE tst.TransactionSplitId = sn.Id
              AND ISNULL(ts.ExcludeFromReporting, 0) = 0
        )
    )
    SELECT
        t.TransactionTypeId AS TransactionType,
        SUM(CASE WHEN t.TransactionTypeId = 2 THEN -CAST(sn.NetAmount AS DECIMAL(12,4)) ELSE CAST(sn.NetAmount AS DECIMAL(12,4)) END) AS Total
    FROM dbo.[Transaction] t
    JOIN SplitNet sn ON sn.TransactionId = t.TransactionId
    WHERE t.AccountId = @AccountId
      AND t.TransactionTime >= @StartDate AND t.TransactionTime < DATEADD(day, 1, @EndDate)
      AND t.ExcludeFromReporting = 0
    GROUP BY t.TransactionTypeId;
END
