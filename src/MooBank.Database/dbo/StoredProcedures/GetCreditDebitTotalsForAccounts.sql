CREATE PROCEDURE dbo.GetCreditDebitTotalsForAccounts
    @AccountIds dbo.GuidList READONLY,
    @StartDate date,
    @EndDate date
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @EndDate = LEAST(@EndDate, CAST(GETDATE() as DATE));

    -- Use TransactionSplitNetAmounts view to aggregate per transaction
    -- A split whose tag is excluded from reporting contributes nothing, so a transfer
    -- leaves the totals instead of inflating both sides, while the other splits of a
    -- part-tagged transaction stay. Mirrors #EligibleTags in GetTransactionTotalsByTag.
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
    )
    SELECT
        t.AccountId,
        t.TransactionTypeId AS TransactionType,
        SUM(CASE WHEN t.TransactionTypeId = 2 THEN -CAST(sn.NetAmount AS DECIMAL(12,4)) ELSE CAST(sn.NetAmount AS DECIMAL(12,4)) END) AS Total
    FROM dbo.[Transaction] t
    JOIN @AccountIds a ON a.Id = t.AccountId
    JOIN SplitNet sn ON sn.TransactionId = t.TransactionId
    WHERE t.TransactionTime >= @StartDate AND t.TransactionTime < DATEADD(day, 1, @EndDate)
      AND t.ExcludeFromReporting = 0
    GROUP BY t.AccountId, t.TransactionTypeId;
END
