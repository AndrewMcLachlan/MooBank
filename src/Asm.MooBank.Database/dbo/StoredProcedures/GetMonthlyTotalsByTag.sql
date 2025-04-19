CREATE PROCEDURE dbo.GetMonthlyTotalsForTag
    @AccountId UNIQUEIDENTIFIER,
    @StartDate date,
    @EndDate date,
    @TagId int,
    @TransactionType varchar(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH TagClosure AS (
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
    ExplicitTaggedSplits AS (
        SELECT tst.TransactionSplitId, tst.TagId
        FROM   dbo.TransactionSplitTag tst
        JOIN   dbo.TagSettings ts ON tst.TagId = ts.TagId
        JOIN   dbo.Tag tg ON tg.Id = tst.TagId
        WHERE  ts.ExcludeFromReporting = 0
          AND  tg.Deleted = 0
    ),
    SplitAncestors AS (
        SELECT
            ets.TransactionSplitId,
            ta.AncestorId AS TagId
        FROM   ExplicitTaggedSplits ets
        JOIN   TagAndAncestors ta ON ta.TagId = ets.TagId
        JOIN   dbo.TagSettings ts ON ta.AncestorId = ts.TagId
        WHERE  ts.ExcludeFromReporting = 0
          AND  ta.AncestorId = @TagId
    ),
    SplitTagAmounts AS (
        SELECT DISTINCT
            ts.Id AS TransactionSplitId,
            ts.Amount,
            ts.NetAmount,
            DATEFROMPARTS(YEAR(t.TransactionTime), MONTH(t.TransactionTime), 1) AS Month
        FROM   dbo.[Transaction] t
        JOIN   dbo.TransactionSplitNetAmounts ts ON ts.TransactionId = t.TransactionId
        JOIN   SplitAncestors sa ON sa.TransactionSplitId = ts.Id
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
    SELECT
        sta.Month,
        SUM(sta.Amount) AS GrossAmount,
        SUM(sta.NetAmount) AS NetAmount
    FROM SplitTagAmounts sta
    GROUP BY sta.Month
    ORDER BY sta.Month;
END
