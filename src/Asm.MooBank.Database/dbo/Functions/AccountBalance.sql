CREATE FUNCTION [dbo].[AccountBalance]
(
    @AccountId UNIQUEIDENTIFIER NULL
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN ISNULL((SELECT SUM(Amount) FROM [Transaction] WHERE AccountId = @AccountId), 0)
END
