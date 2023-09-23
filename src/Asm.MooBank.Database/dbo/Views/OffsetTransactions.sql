CREATE VIEW [dbo].[OffsetTransactions] AS
    SELECT * FROM [Transaction] t
    WHERE ExcludeFromReporting = 0
    AND
    t.TransactionId NOT IN (SELECT OffsetTransactionId FROM [TransactionOffset])
