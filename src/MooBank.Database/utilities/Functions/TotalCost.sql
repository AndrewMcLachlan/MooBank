CREATE FUNCTION [utilities].[TotalCost]
(
    @BillId INT
)
RETURNS DECIMAL(12,4)
AS
BEGIN
DECLARE @Result DECIMAL(12,4)
    -- SUM matters: a bill has one Period per tariff, so a bill spanning a price change has several.
    -- Without it, `SELECT @Result = <expr>` assigns once per row and keeps only the last one, making
    -- the bill cost whichever period happened to be read last rather than the whole bill.
    SELECT @Result = SUM((ISNULL(s.ChargePerDay, 0) * p.Days) + ISNULL(u.Cost, 0)) FROM [utilities].[Period] p
    LEFT JOIN [utilities].[ServiceCharge] s ON p.Id = s.PeriodId
    LEFT JOIN [utilities].[Usage] u ON p.Id = u.PeriodId
    WHERE p.BillId = @BillId

    RETURN @Result
END