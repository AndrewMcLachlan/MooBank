CREATE FUNCTION [dbo].[TransactionSplitNetAmount]
(
    @TransactionSplitId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN ISNULL((SELECT SUM(tso.Amount) FROM [TransactionSplitOffset] tso WHERE tso.TransactionSplitId = @TransactionSplitId), 0) + @Amount
END
