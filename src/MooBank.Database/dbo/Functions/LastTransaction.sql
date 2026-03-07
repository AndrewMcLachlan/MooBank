CREATE FUNCTION [dbo].[LastTransaction]
(
    @AccountId UNIQUEIDENTIFIER NULL
)
RETURNS DATE
AS
BEGIN
    RETURN (SELECT MAX(TransactionTime) FROM [Transaction] WHERE AccountId = @AccountId)
END
