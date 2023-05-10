CREATE VIEW [dbo].[OffsetTransactions] AS
    SELECT * FROM [Transaction] t
    WHERE ExcludeFromReporting = 0
    AND
    t.TransactionId NOT IN (SELECT OffsetByTransactionId FROM [Transaction] WHERE OffsetByTransactionId IS NOT NULL)
