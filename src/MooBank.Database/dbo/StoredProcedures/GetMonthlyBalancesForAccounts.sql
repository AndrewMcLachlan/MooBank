CREATE PROCEDURE dbo.GetMonthlyBalancesForAccounts
    @AccountIds dbo.GuidList READONLY,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @End DATE = LEAST(@EndDate, CAST(GETDATE() as DATE));

    -- Clamp the start date per account to its first transaction, mirroring dbo.GetMonthlyBalances
    SELECT t.AccountId, GREATEST(@StartDate, CAST(MIN(t.TransactionTime) AS DATE)) AS StartDate
    INTO #AccountStarts
    FROM dbo.[Transaction] t
    JOIN @AccountIds a ON a.Id = t.AccountId
    GROUP BY t.AccountId;

    -- Generate calendar month-ends between each account's start and the end date
    WITH MonthEnds AS (
        SELECT AccountId, EOMONTH(StartDate) AS PeriodEnd
        FROM #AccountStarts
        UNION ALL
        SELECT AccountId, EOMONTH(DATEADD(MONTH, 1, PeriodEnd))
        FROM MonthEnds
        WHERE PeriodEnd < @End
    )

    SELECT
        me.AccountId,
        me.PeriodEnd,
        Balance = ISNULL(
            (
                SELECT SUM(t.Amount)
                FROM dbo.[Transaction] t
                WHERE t.AccountId = me.AccountId
                  AND t.TransactionTime < DATEADD(DAY, 1, me.PeriodEnd)
            ),
            0
        )
    FROM MonthEnds me
    ORDER BY me.AccountId, me.PeriodEnd
    OPTION (MAXRECURSION 1000);

    DROP TABLE #AccountStarts;
END
