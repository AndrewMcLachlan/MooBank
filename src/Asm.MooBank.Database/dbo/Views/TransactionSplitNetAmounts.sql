CREATE VIEW [dbo].[TransactionSplitNetAmounts]
AS
    SELECT
        ts.Id,
        ts.TransactionId,
        ts.Amount,
        Amount
        - ISNULL((SELECT SUM(tso.Amount) FROM [dbo].[TransactionSplitOffset] tso WHERE tso.TransactionSplitId = Id), 0)
        - ISNULL((SELECT SUM(tso.Amount) FROM [dbo].[TransactionSplitOffset] tso WHERE tso.OffsetTransactionId = TransactionId), 0) as NetAmount
    FROM dbo.TransactionSplit ts
GO
