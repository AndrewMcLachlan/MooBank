CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @TransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN ISNULL((SELECT SUM(tso.Amount) FROM [TransactionSplitOffset] tso INNER JOIN [TransactionSplit] ts ON ts.Id = tso.TransactionSplitId WHERE ts.TransactionId = @TransactionId), 0) + @Amount
END
