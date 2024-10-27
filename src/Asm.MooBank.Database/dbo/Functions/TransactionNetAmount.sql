CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @TransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(12,4)
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    RETURN ISNULL((SELECT SUM([dbo].[TransactionSplitNetAmount](@TransactionId, ts.Id,  ts.Amount)) FROM [TransactionSplitNetAmounts] ts WHERE ts.TransactionId = @TransactionId), @Amount)
END
