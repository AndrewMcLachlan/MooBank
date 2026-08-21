CREATE FUNCTION [utilities].[TotalCost]
(
    @BillId INT
)
RETURNS DECIMAL(12,4)
AS
BEGIN
DECLARE @Result DECIMAL(12,4)
    -- A billing period counts both end days -- 26 Jun to 25 Jul is 30 -- which is DaysInclusive,
    -- not the DATEDIFF in Days beside it.
    SELECT @Result = SUM((ISNULL(s.ChargePerDay, 0) * p.DaysInclusive) + ISNULL(u.Cost, 0)) FROM [utilities].[Period] p
    LEFT JOIN [utilities].[ServiceCharge] s ON p.Id = s.PeriodId
    LEFT JOIN [utilities].[Usage] u ON p.Id = u.PeriodId
    WHERE p.BillId = @BillId

    RETURN @Result
END