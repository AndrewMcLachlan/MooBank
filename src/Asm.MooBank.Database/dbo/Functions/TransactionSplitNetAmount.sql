CREATE FUNCTION [dbo].[TransactionSplitNetAmount]
(
    @TransactionSplitId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(12,4)
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    RETURN ISNULL((SELECT SUM(tso.Amount) FROM [TransactionSplitOffset] tso WHERE tso.TransactionSplitId = @TransactionSplitId), 0) + @Amount
END
