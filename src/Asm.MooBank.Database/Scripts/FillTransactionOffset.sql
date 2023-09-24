INSERT INTO TransactionOffset
SELECT t.TransactionId, t.OffsetByTransactionId, LEAST(ABS(t.Amount), t2.Amount) FROM [Transaction] t
INNER JOIN [Transaction] t2 ON t2.TransactionId = t.OffsetByTransactionId
WHERE t.OffsetByTransactionId IS NOT NULL 
