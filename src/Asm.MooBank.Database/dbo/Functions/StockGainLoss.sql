CREATE FUNCTION [dbo].[StockGainLoss]
(
    @AccountId UNIQUEIDENTIFIER NULL,
    @CurrentPrice DECIMAL(12, 4) NULL
)
RETURNS DECIMAL(12,4)
AS
BEGIN

DECLARE @res DECIMAL(12,4)

;WITH GandL AS (
  SELECT CASE WHEN TransactionTypeId = 1 THEN SUM(Quantity*Price) ELSE 0 END as bought, CASE WHEN TransactionTypeId = 2 THEN SUM(Quantity*Price) ELSE 0 END as sold
    FROM StockTransaction
    GROUP BY TransactionTypeId
)

SELECT @res = SUM(bought) - SUM(sold) FROM GandL

RETURN dbo.StockValue(@AccountId, @CurrentPrice) - ISNULL(@res,0)

END