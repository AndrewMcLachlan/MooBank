CREATE PROCEDURE dbo.GetMonthlyTotalsForTag
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date,
    @TagId int,
    @TransactionTypeId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @StartDate = GREATEST(@StartDate, (SELECT Min(TransactionTime) FROM [Transaction] WHERE AccountId = @AccountId))
    SELECT @EndDate = LEAST(@EndDate, CAST(GETDATE() as DATE));

    -- 1. Cache eligible tags
    SELECT t.Id
    INTO #EligibleTags
    FROM dbo.Tag t
    JOIN dbo.TagSettings ts ON t.Id = ts.TagId
    WHERE t.Deleted = 0 AND ts.ExcludeFromReporting = 0;

    CREATE INDEX IX_EligibleTags_Id ON #EligibleTags(Id);

    -- 2. Build DAG closure for selected tag only
    ;WITH Descendants AS (
        SELECT tt.PrimaryTagId
        FROM dbo.TagTag tt
        WHERE tt.SecondaryTagId = @TagId
        UNION ALL
        SELECT tt.PrimaryTagId
        FROM Descendants d
        JOIN dbo.TagTag tt ON tt.SecondaryTagId = d.PrimaryTagId
    )
    SELECT DISTINCT TagId
    INTO #TagDescendants
    FROM (
        SELECT PrimaryTagId AS TagId
        FROM Descendants
        UNION
        SELECT @TagId
    ) AS Combined
    WHERE TagId IN (SELECT Id FROM #EligibleTags);

    CREATE UNIQUE CLUSTERED INDEX IX_TagDescendants_TagId ON #TagDescendants(TagId);

    -- 3. Build valid tagged split + ancestor pairs
    SELECT tst.TransactionSplitId, ta.AncestorId AS TagId
    INTO #SplitAncestors
    FROM dbo.TransactionSplitTag tst
    JOIN (
        SELECT t.Id AS TagId, t.Id AS AncestorId
        FROM #EligibleTags t
        UNION
        SELECT tc.TagId, tc.AncestorId
        FROM (
            SELECT tt.PrimaryTagId AS TagId, tt.SecondaryTagId AS AncestorId
            FROM dbo.TagTag tt
            UNION ALL
            SELECT ttc.PrimaryTagId AS TagId, tt2.SecondaryTagId AS AncestorId
            FROM dbo.TagTag tt2
            JOIN dbo.TagTag ttc ON tt2.PrimaryTagId = ttc.SecondaryTagId
        ) AS tc(TagId, AncestorId)
        WHERE tc.AncestorId IN (SELECT Id FROM #EligibleTags)
    ) ta ON ta.TagId = tst.TagId
    WHERE ta.AncestorId IN (SELECT TagId FROM #TagDescendants);

    CREATE INDEX IX_SplitAncestors_SplitId ON #SplitAncestors(TransactionSplitId);

    -- 4. Join transactions and compute net/gross by month
    SELECT DISTINCT
        DATEFROMPARTS(YEAR(t.TransactionTime), MONTH(t.TransactionTime), 1) AS Month,
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

    -- 5. Final rollup by month
    SELECT
        sta.Month,
        SUM(sta.Amount) AS GrossAmount,
        SUM(sta.NetAmount) AS NetAmount
    FROM #SplitTagAmounts sta
    GROUP BY sta.Month
    ORDER BY sta.Month;

    DROP TABLE #EligibleTags, #TagDescendants, #SplitAncestors, #SplitTagAmounts;
END
