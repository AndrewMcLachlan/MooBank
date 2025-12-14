-- Populate new columns
UPDATE ti
SET 
    Balance = ISNULL(t.Balance, 0),
    LastTransaction = CAST(t.LastTransaction AS DATE)
FROM [dbo].[TransactionInstrument] ti
OUTER APPLY (
    SELECT 
        SUM(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END) as Balance,
        MAX(TransactionTime) as LastTransaction
    FROM [dbo].[Transaction] t
    WHERE t.AccountId = ti.InstrumentId
) t;
GO
