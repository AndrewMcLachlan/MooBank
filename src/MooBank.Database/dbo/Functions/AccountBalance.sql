CREATE FUNCTION [dbo].[AccountBalance]
(
    @AccountId UNIQUEIDENTIFIER NULL
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    RETURN ISNULL((SELECT SUM(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END) FROM [Transaction] WHERE AccountId = @AccountId), 0)
END
