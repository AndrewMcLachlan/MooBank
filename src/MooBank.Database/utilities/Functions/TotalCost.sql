CREATE FUNCTION [utilities].[TotalCost]
(
    @BillId INT
)
RETURNS DECIMAL(12,4)
AS
BEGIN
DECLARE @Result DECIMAL(12,4)
    -- Service charges and usages are summed over separate sets rather than joined: a period can
    -- carry several of each -- water and sewerage, consumption and export -- and joining them
    -- multiplies one by the count of the other.
    --
    -- A billing period counts both end days -- 26 Jun to 25 Jul is 30 -- which is DaysInclusive,
    -- not the DATEDIFF in Days beside it.
    SELECT @Result = SUM(Amount)
    FROM (
        SELECT s.[ChargePerDay] * p.[DaysInclusive] AS Amount
        FROM [utilities].[ServiceCharge] s
        INNER JOIN [utilities].[Period] p ON p.[Id] = s.[PeriodId]
        WHERE p.[BillId] = @BillId

        UNION ALL

        SELECT u.[Cost]
        FROM [utilities].[Usage] u
        INNER JOIN [utilities].[Period] p ON p.[Id] = u.[PeriodId]
        WHERE p.[BillId] = @BillId
    ) amounts

    RETURN @Result
END
