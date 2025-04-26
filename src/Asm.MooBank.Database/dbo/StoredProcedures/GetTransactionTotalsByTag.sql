CREATE PROCEDURE dbo.GetTransactionTotalsByTag
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date,
    @RootTagId int = NULL,
    @TransactionTypeId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Cache eligible tags (non-deleted and not excluded from reporting)
    SELECT t.Id, t.Name
    INTO #EligibleTags
    FROM dbo.Tag t
    JOIN dbo.TagSettings ts ON t.Id = ts.TagId
    WHERE t.Deleted = 0 AND ts.ExcludeFromReporting = 0;

    CREATE INDEX IX_EligibleTags_Id ON #EligibleTags(Id);

    -- 2. Build tag closure (DAG ancestor map)
    ;WITH TagClosure AS (
        SELECT tt.PrimaryTagId AS TagId, tt.SecondaryTagId AS AncestorId
        FROM dbo.TagTag tt
        UNION ALL
        SELECT tc.TagId, tt.SecondaryTagId
        FROM TagClosure tc
        JOIN dbo.TagTag tt ON tt.PrimaryTagId = tc.AncestorId
    )
    SELECT t.Id AS TagId, t.Id AS AncestorId
    INTO #TagAndAncestors
    FROM #EligibleTags t
    UNION
    SELECT tc.TagId, tc.AncestorId
    FROM TagClosure tc
    JOIN #EligibleTags et ON tc.AncestorId = et.Id;

    CREATE INDEX IX_TagAndAncestors_TagId ON #TagAndAncestors(TagId);
    CREATE INDEX IX_TagAndAncestors_AncestorId ON #TagAndAncestors(AncestorId);

    -- 3. Build SplitAncestors (only valid split/tag combos)
    SELECT tst.TransactionSplitId, ta.AncestorId AS TagId
    INTO #SplitAncestors
    FROM dbo.TransactionSplitTag tst
    JOIN #TagAndAncestors ta ON ta.TagId = tst.TagId;

    CREATE INDEX IX_SplitAncestors_SplitId ON #SplitAncestors(TransactionSplitId);
    CREATE INDEX IX_SplitAncestors_TagId ON #SplitAncestors(TagId);

    -- 4. Join splits with amounts and filter by transaction criteria
    SELECT DISTINCT
        sa.TagId,
        ts.Id AS TransactionSplitId,
        ts.Amount,
        ts.Amount
          - ISNULL(o1.TotalOffset, 0)
          - ISNULL(o2.TotalOffset, 0) AS NetAmount
    INTO #SplitTagAmounts
    FROM dbo.[Transaction] t
    JOIN dbo.TransactionSplit ts ON ts.TransactionId = t.TransactionId
    JOIN #SplitAncestors sa ON sa.TransactionSplitId = ts.Id
    LEFT JOIN (
        SELECT TransactionSplitId, SUM(Amount) AS TotalOffset
        FROM dbo.TransactionSplitOffset
        GROUP BY TransactionSplitId
    ) o1 ON o1.TransactionSplitId = ts.Id
    LEFT JOIN (
        SELECT OffsetTransactionId, SUM(Amount) AS TotalOffset
        FROM dbo.TransactionSplitOffset
        GROUP BY OffsetTransactionId
    ) o2 ON o2.OffsetTransactionId = t.TransactionId
    WHERE t.AccountId = @AccountId
      AND t.TransactionTime >= @StartDate AND t.TransactionTime <= @EndDate
      AND t.ExcludeFromReporting = 0
      AND (@TransactionTypeId IS NULL OR @TransactionTypeId = 0 OR t.TransactionTypeId  = @TransactionTypeId);

    CREATE INDEX IX_SplitTagAmounts_TagId ON #SplitTagAmounts(TagId);

    -- 5. Final rollup with HasChildren logic
    SELECT
        tg.Id AS TagId,
        tg.Name AS TagName,
        CAST(CASE
            WHEN EXISTS (
                SELECT 1
                FROM dbo.TagTag tt
                JOIN #SplitTagAmounts staChild ON staChild.TagId = tt.PrimaryTagId
                WHERE tt.SecondaryTagId = tg.Id
            ) THEN 1 ELSE 0 END AS bit) AS HasChildren,
        SUM(sta.Amount) AS GrossAmount,
        SUM(sta.NetAmount) AS NetAmount
    FROM #SplitTagAmounts sta
    JOIN #EligibleTags tg ON tg.Id = sta.TagId
    WHERE (
        @RootTagId IS NULL
        AND NOT EXISTS (
            SELECT 1 FROM dbo.TagTag tt
            WHERE tt.PrimaryTagId = tg.Id
        )
    )
    OR EXISTS (
        SELECT 1 FROM dbo.TagTag tt
        WHERE tt.PrimaryTagId = tg.Id AND tt.SecondaryTagId = @RootTagId
    )
    GROUP BY tg.Id, tg.Name
    ORDER BY GrossAmount DESC;

    DROP TABLE #EligibleTags, #TagAndAncestors, #SplitAncestors, #SplitTagAmounts;
END
