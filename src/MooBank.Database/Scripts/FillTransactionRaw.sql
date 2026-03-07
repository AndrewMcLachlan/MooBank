INSERT INTO TransactionSplitOffset SELECT ts.Id, tof.OffsetTransactionId, tof.Amount FROM TransactionSplit ts INNER JOIN TransactionOffset tof ON ts.TransactionId = tof.TransactionId


