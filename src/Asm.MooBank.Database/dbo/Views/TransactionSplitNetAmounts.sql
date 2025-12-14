CREATE VIEW [dbo].[TransactionSplitNetAmounts]
AS
SELECT
    ts.Id,
    ts.TransactionId,
    ts.Amount,
    ts.Amount
    - ISNULL(OffsetOut.TotalOffset, 0)
    - ISNULL(OffsetIn.TotalOffset, 0) as NetAmount
FROM dbo.TransactionSplit ts
LEFT JOIN (
    SELECT TransactionSplitId, SUM(Amount) as TotalOffset
    FROM dbo.TransactionSplitOffset
    GROUP BY TransactionSplitId
) AS OffsetOut ON ts.Id = OffsetOut.TransactionSplitId
LEFT JOIN (
    SELECT OffsetTransactionId, SUM(Amount) as TotalOffset
    FROM dbo.TransactionSplitOffset
    GROUP BY OffsetTransactionId
) AS OffsetIn ON ts.TransactionId = OffsetIn.OffsetTransactionId
GO
