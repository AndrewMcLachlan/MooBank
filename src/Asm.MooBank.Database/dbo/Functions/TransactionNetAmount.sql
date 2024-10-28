CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @TransactionTypeId INT,
    @TransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(12,4)
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    DECLARE @AbsAmount DECIMAL(12,4) = ISNULL((SELECT SUM([dbo].[TransactionSplitNetAmount](@TransactionId, ts.Id,  ts.Amount)) FROM [TransactionSplitNetAmounts] ts WHERE ts.TransactionId = @TransactionId), @Amount)

    IF @TransactionTypeId % 2 = 0
    SET @AbsAmount = -@AbsAmount

    RETURN @AbsAmount
END
