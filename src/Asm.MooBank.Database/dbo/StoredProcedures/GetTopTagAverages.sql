CREATE PROCEDURE dbo.GetTopTagAverages
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date,
    @Period varchar(10) = 'Monthly',   -- 'Monthly' or 'Yearly'
    @TransactionType int = 2         -- 1 = Credit, 2 = Debit, NULL = All
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PeriodCount int =
        CASE
            WHEN @Period = 'Yearly' THEN DATEDIFF(YEAR, @StartDate, DATEADD(DAY, 1, @EndDate))
            ELSE DATEDIFF(MONTH, @StartDate, DATEADD(DAY, 1, @EndDate))
        END;

    -- 1. Get eligible tags
    SELECT t.Id, t.Name
    INTO #Tags
    FROM dbo.Tag t
    JOIN dbo.TagSettings ts ON t.Id = ts.TagId
    WHERE t.Deleted = 0 AND ts.ExcludeFromReporting = 0;

    CREATE INDEX IX_Tags_Id ON #Tags(Id);

    -- 2. Get assigned tag splits
    SELECT tst.TransactionSplitId, tst.TagId
    INTO #SplitTags
    FROM dbo.TransactionSplitTag tst
    JOIN #Tags t ON t.Id = tst.TagId;

    CREATE INDEX IX_SplitTags_SplitId ON #SplitTags(TransactionSplitId);

    -- 3. Join transactions, calculate net amount (as absolute value)
    SELECT
        st.TagId,
        t.Name,
        NetAmount = ts.Amount
                  - ISNULL(so1.TotalOffset, 0)
                  - ISNULL(so2.TotalOffset, 0)
    INTO #TagAmounts
    FROM #SplitTags st
    JOIN dbo.TransactionSplit ts ON ts.Id = st.TransactionSplitId
    JOIN dbo.[Transaction] tx ON tx.TransactionId = ts.TransactionId
    JOIN #Tags t ON t.Id = st.TagId
    LEFT JOIN (
        SELECT TransactionSplitId, SUM(Amount) AS TotalOffset
        FROM dbo.TransactionSplitOffset
        GROUP BY TransactionSplitId
    ) so1 ON so1.TransactionSplitId = ts.Id
    LEFT JOIN (
        SELECT OffsetTransactionId, SUM(Amount) AS TotalOffset
        FROM dbo.TransactionSplitOffset
        GROUP BY OffsetTransactionId
    ) so2 ON so2.OffsetTransactionId = ts.TransactionId
    WHERE tx.AccountId = @AccountId
      AND tx.TransactionTime >= @StartDate AND tx.TransactionTime <= @EndDate
      AND tx.ExcludeFromReporting = 0
      AND (
            @TransactionType IS NULL
         OR (@TransactionType = 1 AND tx.TransactionTypeId % 2 = 1)
         OR (@TransactionType = 2 AND tx.TransactionTypeId % 2 = 0)
      );

    -- 4. Aggregate by tag
    SELECT
        TagId,
        Name,
        CAST(SUM(NetAmount) / NULLIF(@PeriodCount, 0) AS decimal(18,4)) AS Average
    FROM #TagAmounts
    GROUP BY TagId, Name
    ORDER BY Average DESC;

    DROP TABLE #Tags, #SplitTags, #TagAmounts;
END