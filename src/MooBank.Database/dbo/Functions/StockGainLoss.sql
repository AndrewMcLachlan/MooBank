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
  SELECT 
    CASE WHEN TransactionTypeId = 1 THEN CAST(SUM(CAST(Quantity AS DECIMAL(12,4)) * Price) AS DECIMAL(12,4)) ELSE CAST(0 AS DECIMAL(12,4)) END as bought, 
    CASE WHEN TransactionTypeId = 2 THEN CAST(SUM(CAST(Quantity AS DECIMAL(12,4)) * Price) AS DECIMAL(12,4)) ELSE CAST(0 AS DECIMAL(12,4)) END as sold
    FROM StockTransaction
    WHERE AccountId = @AccountId
    GROUP BY TransactionTypeId
)

SELECT @res = SUM(bought) - SUM(sold) FROM GandL

RETURN dbo.StockValue(@AccountId, @CurrentPrice) - ISNULL(@res,0)

END
