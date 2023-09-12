CREATE FUNCTION [dbo].[CheckOffsetAmount]
(
    @TransactionId UNIQUEIDENTIFIER,
    @OffsetTransactionId UNIQUEIDENTIFIER,
    @Amount DECIMAL(19,4)
)
RETURNS BIT
AS
BEGIN

    DECLARE @TransactionAmount DECIMAL(19,4)
    DECLARE @OffsetTransactionAmount DECIMAL(19,4)

    SELECT @TransactionAmount = Amount FROM [Transaction] WHERE TransactionId = @TransactionId
    SELECT @OffsetTransactionAmount = Amount FROM [Transaction] WHERE TransactionId = @OffsetTransactionId

    RETURN CASE WHEN @Amount <= ABS(@TransactionAmount) AND @Amount <= @OffsetTransactionAmount THEN 1 ELSE 0 END

END