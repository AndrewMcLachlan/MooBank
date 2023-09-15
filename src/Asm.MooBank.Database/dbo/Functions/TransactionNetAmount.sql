CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @OffsetByTransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN ISNULL((SELECT SUM(Amount) FROM [TransactionOffset] WHERE TransactionId = TransactionId), 0) + @Amount
END
