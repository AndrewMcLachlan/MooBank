CREATE FUNCTION [dbo].[CheckSplitAmount]
(
    @TransactionId UNIQUEIDENTIFIER,
    @Amount DECIMAL(19,4)
)
RETURNS BIT
AS
BEGIN

    DECLARE @TransactionAmount DECIMAL(19,4)

    SELECT @TransactionAmount = Amount FROM [Transaction] WHERE TransactionId = @TransactionId

    RETURN CASE WHEN ABS(@Amount) <= ABS(@TransactionAmount) THEN 1 ELSE 0 END

END