CREATE FUNCTION [dbo].[StockQuantity]
(
    @AccountId UNIQUEIDENTIFIER NULL
)
RETURNS INT
AS
BEGIN
    RETURN ISNULL((SELECT SUM(
    CASE WHEN
    TransactionTypeId = 1 THEN Quantity
    WHEN TransactionTypeId = 2 THEN -Quantity
    ELSE 0 END
    ) FROM StockTransaction WHERE AccountId = @AccountId), 0)
END