CREATE VIEW [dbo].[TransactionSplitNetAmounts] AS
    SELECT
        *,
        [dbo].TransactionSplitNetAmount(ts.TransactionId, ts.Id, ts.Amount) AS NetAmount
    FROM
        [dbo].[TransactionSplit] ts
