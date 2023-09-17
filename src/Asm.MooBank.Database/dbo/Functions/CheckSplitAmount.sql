CREATE FUNCTION [dbo].[CheckSplitAmount]
(
    @TransactionId UNIQUEIDENTIFIER,
    @Amount DECIMAL(19,4)
)
RETURNS BIT
AS
BEGIN

    DECLARE @TransactionAmount DECIMAL(10,2)
    DECLARE @TotalSplit DECIMAL(10,2)

    SELECT @TransactionAmount = Amount FROM [Transaction] WHERE TransactionId = @TransactionId

    SELECT @TotalSplit = SUM(Amount) FROM [TransactionSplit] WHERE TransactionId = @TransactionId

    RETURN CASE WHEN ABS(@Amount) <= ABS(@TransactionAmount) AND ABS(@TotalSplit) + ABS(@Amount) <= ABS(@TransactionAmount) THEN 1 ELSE 0 END

END
