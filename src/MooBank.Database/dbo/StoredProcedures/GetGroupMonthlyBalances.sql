CREATE PROCEDURE dbo.GetGroupMonthlyBalances
    @GroupId UNIQUEIDENTIFIER,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Get all instruments in the group
    DECLARE @Start DATE, @End DATE
    SELECT @Start = @StartDate
    SELECT @End = LEAST(@EndDate, CAST(GETDATE() as DATE))

    -- Get minimum transaction date across all accounts in the group
    DECLARE @MinTransactionDate DATE;

    WITH instruments AS (
        SELECT io.InstrumentId
        FROM InstrumentOwner io
        WHERE ISNULL(io.GroupId, '00000000-0000-0000-0000-000000000000') = @GroupId
        UNION
        SELECT iv.InstrumentId
        FROM InstrumentViewer iv
        WHERE ISNULL(iv.GroupId, '00000000-0000-0000-0000-000000000000') = @GroupId
    )
    SELECT @MinTransactionDate = MIN(CAST(t.TransactionTime AS DATE))
    FROM [Transaction] t
    INNER JOIN instruments i ON t.AccountId = i.InstrumentId

    SELECT @Start = GREATEST(@Start, ISNULL(@MinTransactionDate, @Start));

    -- Generate calendar month-ends between start and end
    WITH MonthEnds AS (
        SELECT EOMONTH(@Start) AS PeriodEnd
        UNION ALL
        SELECT EOMONTH(DATEADD(MONTH, 1, PeriodEnd))
        FROM MonthEnds
        WHERE PeriodEnd < @End
    ),
    GroupInstruments AS (
        SELECT io.InstrumentId
        FROM InstrumentOwner io
        WHERE ISNULL(io.GroupId, '00000000-0000-0000-0000-000000000000') = @GroupId
        UNION
        SELECT iv.InstrumentId
        FROM InstrumentViewer iv
        WHERE ISNULL(iv.GroupId, '00000000-0000-0000-0000-000000000000') = @GroupId
    )
    SELECT
        me.PeriodEnd,
        Balance = ISNULL(
            (
                SELECT SUM(t.Amount)
                FROM dbo.[Transaction] t
                INNER JOIN GroupInstruments gi ON t.AccountId = gi.InstrumentId
                WHERE t.TransactionTime <= me.PeriodEnd
            ),
            0
        )
    FROM MonthEnds me
    ORDER BY me.PeriodEnd

OPTION (MAXRECURSION 1000);
END
