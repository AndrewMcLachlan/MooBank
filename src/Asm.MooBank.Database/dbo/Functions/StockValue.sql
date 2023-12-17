CREATE FUNCTION [dbo].[StockValue]
(
    @AccountId UNIQUEIDENTIFIER NULL,
    @CurrentPrice DECIMAL(18, 2) NULL
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    RETURN [dbo].[StockQuantity](@AccountId) * @CurrentPrice
END