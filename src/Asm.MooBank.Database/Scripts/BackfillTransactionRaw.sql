INSERT INTO ing.TransactionRaw SELECT NEWID(), [TransactionId], [AccountId], TransactionTime, Description, CASE WHEN Amount > 0 THEN Amount ELSE NULL END, CASE WHEN Amount < 0 THEN Amount ELSE NULL END, NULL, SYSDATETIME() FROM [Transaction] WHERE AccountId IN ('6b4ae4d9-d4ba-41f7-80e6-076863df9407', '3740bb76-e226-4bce-a8ab-3203824a6fa9')
