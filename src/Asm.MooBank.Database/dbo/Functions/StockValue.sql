CREATE FUNCTION [dbo].[StockValue]
(
    @AccountId UNIQUEIDENTIFIER NULL,
    @CurrentPrice DECIMAL(12, 4) NULL
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    RETURN CAST([dbo].[StockQuantity](@AccountId) AS DECIMAL(12,4)) * @CurrentPrice
END