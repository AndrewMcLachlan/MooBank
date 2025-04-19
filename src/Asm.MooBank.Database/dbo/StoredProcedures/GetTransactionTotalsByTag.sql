CREATE PROCEDURE dbo.GetTransactionTotalsByTag
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date,
    @RootTagId int = NULL,
    @TransactionType varchar(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* 1. All descendants of a given tag (for filtering) */
    ;WITH Descendants AS (
        SELECT tt.PrimaryTagId
        FROM   dbo.TagTag tt
        WHERE  tt.SecondaryTagId = @RootTagId
        UNION ALL
        SELECT tt.PrimaryTagId
        FROM   Descendants d
        JOIN   dbo.TagTag tt ON tt.SecondaryTagId = d.PrimaryTagId
    ),
    SubtreeTags AS (
        SELECT @RootTagId AS TagId
        WHERE  @RootTagId IS NOT NULL
        UNION
        SELECT PrimaryTagId FROM Descendants
    ),

    /* 2. DAG ancestor closure */
    TagClosure AS (
        SELECT tt.PrimaryTagId AS TagId, tt.SecondaryTagId AS AncestorId
        FROM   dbo.TagTag tt
        UNION ALL
        SELECT tc.TagId, tt.SecondaryTagId
        FROM   TagClosure tc
        JOIN   dbo.TagTag tt ON tt.PrimaryTagId = tc.AncestorId
    ),
    TagAndAncestors AS (
        SELECT Id AS TagId, Id AS AncestorId
        FROM   dbo.Tag
        WHERE  Deleted = 0
        UNION
        SELECT TagId, AncestorId
        FROM   TagClosure
        WHERE  AncestorId IN (SELECT Id FROM dbo.Tag WHERE Deleted = 0)
    ),

    /* 3. Explicitly tagged splits, only using included + non-deleted tags */
    ExplicitTaggedSplits AS (
        SELECT tst.TransactionSplitId, tst.TagId
        FROM   dbo.TransactionSplitTag tst
        JOIN   dbo.TagSettings ts ON tst.TagId = ts.TagId
        JOIN   dbo.Tag tg ON tg.Id = tst.TagId
        WHERE  ts.ExcludeFromReporting = 0
          AND  tg.Deleted = 0
    ),

    /* 4. Ancestors of those tags */
    SplitAncestors AS (
        SELECT
            ets.TransactionSplitId,
            ta.AncestorId AS TagId
        FROM   ExplicitTaggedSplits ets
        JOIN   TagAndAncestors ta ON ta.TagId = ets.TagId
        JOIN   dbo.TagSettings ts ON ta.AncestorId = ts.TagId
        WHERE  ts.ExcludeFromReporting = 0
          AND  ta.AncestorId IN (SELECT Id FROM dbo.Tag WHERE Deleted = 0)
          AND (
                @RootTagId IS NULL
             OR EXISTS (
                   SELECT 1 FROM SubtreeTags st WHERE st.TagId = ta.AncestorId
             )
          )
    ),

    /* 5. One row per split and ancestor tag */
    SplitTagAmounts AS (
        SELECT DISTINCT
            ta.TagId,
            ts.Id AS TransactionSplitId,
            ts.NetAmount,
            ts.Amount
        FROM   dbo.[Transaction] t
        JOIN   dbo.TransactionSplitNetAmounts ts ON ts.TransactionId = t.TransactionId
        JOIN   SplitAncestors ta ON ta.TransactionSplitId = ts.Id
        WHERE  t.TransactionTime >= @StartDate
          AND  t.TransactionTime <= @EndDate
          AND  t.ExcludeFromReporting = 0
          AND  t.AccountId = @AccountId
          AND (
                @TransactionType IS NULL
             OR (@TransactionType = 'Credit' AND t.TransactionTypeId % 2 = 1)
             OR (@TransactionType = 'Debit'  AND t.TransactionTypeId % 2 = 0)
          )
    )

    /* 6. Final DAG-safe rollup + filter to top-level or immediate children */
    SELECT
        tg.Id AS TagId,
        tg.Name AS TagName,
        CAST(CASE
            WHEN EXISTS (
                SELECT 1
                FROM dbo.TagTag tt
                JOIN SplitTagAmounts childSta ON childSta.TagId = tt.PrimaryTagId
                WHERE tt.SecondaryTagId = tg.Id
            ) THEN 1 ELSE 0 END AS bit) AS HasChildren,
        SUM(sta.Amount) AS GrossAmount,
        SUM(sta.NetAmount) AS NetAmount
    FROM   SplitTagAmounts sta
    JOIN   dbo.Tag tg ON tg.Id = sta.TagId
    WHERE
        (
            @RootTagId IS NULL
            AND NOT EXISTS (
                SELECT 1 FROM dbo.TagTag tt
                WHERE tt.PrimaryTagId = tg.Id
            )
        )
        OR
        EXISTS (
            SELECT 1 FROM dbo.TagTag tt
            WHERE tt.PrimaryTagId = tg.Id
              AND tt.SecondaryTagId = @RootTagId
        )
    GROUP BY tg.Id, tg.Name
    ORDER BY NetAmount DESC;
END
