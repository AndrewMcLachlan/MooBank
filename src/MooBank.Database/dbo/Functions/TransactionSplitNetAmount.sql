CREATE FUNCTION [dbo].[TransactionSplitNetAmount]
(
    @TransactionId UNIQUEIDENTIFIER NULL,
    @TransactionSplitId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(12,4)
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    -- Amounts are stored as positives in splits, therefore subtract the offset from the amount.
    DECLARE @OffsetAmount decimal(12,4) = @Amount - ISNULL((SELECT SUM(tso.Amount) FROM [TransactionSplitOffset] tso WHERE tso.TransactionSplitId = @TransactionSplitId), 0)

    SET @OffsetAmount = @OffsetAmount - ISNULL((SELECT SUM(tso.Amount) FROM [TransactionSplitOffset] tso WHERE tso.OffsetTransactionId = @TransactionId), 0)

    RETURN @OffsetAmount
END
