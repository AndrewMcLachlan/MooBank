SELECT t.TransactionId, t.Amount, tt.TransactionTagId FROM [Transaction] t
INNER JOIN TransactionTransactionTag tt ON t.TransactionId = tt.TransactionId

BEGIN TRAN
INSERT INTO [TransactionSplit] SELECT NEWID(), t.TransactionId, t.Amount FROM [Transaction] t WHERE t.TransactionId IN (SELECT DISTINCT TransactionId FROM TransactionTransactionTag)
INSERT INTO [TransactionSplitTag] SELECT ts.Id, tt.TransactionTagId FROM [TransactionTransactionTag] tt INNER JOIN TransactionSplit ts ON tt.TransactionId = ts.TransactionId

COMMIT
