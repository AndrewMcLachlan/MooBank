CREATE FUNCTION [dbo].[StockValue]
(
    @AccountId UNIQUEIDENTIFIER NULL,
    @CurrentPrice DECIMAL(12, 4) NULL
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    RETURN [dbo].[StockQuantity](@AccountId) * @CurrentPrice
END