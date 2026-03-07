BEGIN TRANSACTION;
UPDATE [Transaction] 
SET TransactionSubTypeId = 
CASE WHEN TransactionTypeId IN (3,4) THEN 2
WHEN TransactionTypeId IN (5,6) THEN 3
WHEN TransactionTypeId IN (7,8) THEN 4
ELSE NULL
END

UPDATE [Transaction]
SET TransactionTypeId = 
CASE WHEN TransactionTypeId IN (3,5,7) THEN 1
WHEN TransactionTypeId IN (4,6,8) THEN 2
END