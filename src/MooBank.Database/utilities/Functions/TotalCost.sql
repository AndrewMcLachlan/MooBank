CREATE FUNCTION [utilities].[TotalCost]
(
    @BillId INT
)
RETURNS DECIMAL(12,4)
AS
BEGIN
DECLARE @Result DECIMAL(12,4)
    SELECT @Result = (ISNULL(s.ChargePerDay, 0) * p.Days) + ISNULL(u.Cost, 0) FROM [utilities].[Period] p
    LEFT JOIN [utilities].[ServiceCharge] s ON p.Id = s.PeriodId
    LEFT JOIN [utilities].[Usage] u ON p.Id = u.PeriodId
    WHERE p.BillId = @BillId

    RETURN @Result
END