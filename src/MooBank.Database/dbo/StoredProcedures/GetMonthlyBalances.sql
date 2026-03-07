CREATE PROCEDURE dbo.GetMonthlyBalances
    @AccountId UNIQUEIDENTIFIER,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Start DATE, @End DATE
    SELECT @Start = GREATEST(@StartDate, (SELECT Min(TransactionTime) FROM [Transaction] WHERE AccountId = @AccountId))
    SELECT @End = LEAST(@EndDate, CAST(GETDATE() as DATE));

    -- Generate calendar month-ends between start and end
    WITH MonthEnds AS (
        SELECT EOMONTH(@Start) AS PeriodEnd
        UNION ALL
        SELECT EOMONTH(DATEADD(MONTH, 1, PeriodEnd))
        FROM MonthEnds
        WHERE PeriodEnd < @End
    )

    SELECT
        me.PeriodEnd,
        Balance = ISNULL(
            (
                SELECT SUM(t.Amount)
                FROM dbo.[Transaction] t
                WHERE t.AccountId = @AccountId
                  AND t.TransactionTime <= me.PeriodEnd
            ),
            0
        )
    FROM MonthEnds me
    ORDER BY me.PeriodEnd

OPTION (MAXRECURSION 1000);
END
