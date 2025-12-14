-- Drop computed columns
ALTER TABLE [dbo].[TransactionInstrument] DROP COLUMN [Balance];
ALTER TABLE [dbo].[TransactionInstrument] DROP COLUMN [LastTransaction];

-- Add real columns
ALTER TABLE [dbo].[TransactionInstrument] ADD [Balance] DECIMAL(12, 4) NOT NULL CONSTRAINT DF_TransactionInstrument_Balance DEFAULT 0;
ALTER TABLE [dbo].[TransactionInstrument] ADD [LastTransaction] DATE NULL;
GO

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

-- Create Trigger to maintain Balance and LastTransaction
CREATE OR ALTER TRIGGER [dbo].[TR_Transaction_UpdateBalance]
ON [dbo].[Transaction]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AffectedAccounts TABLE (AccountId UNIQUEIDENTIFIER);

    INSERT INTO @AffectedAccounts
    SELECT AccountId FROM inserted WHERE AccountId IS NOT NULL
    UNION
    SELECT AccountId FROM deleted WHERE AccountId IS NOT NULL;

    UPDATE ti
    SET 
        Balance = ISNULL(t.Balance, 0),
        LastTransaction = CAST(t.LastTransaction AS DATE)
    FROM [dbo].[TransactionInstrument] ti
    INNER JOIN @AffectedAccounts a ON ti.InstrumentId = a.AccountId
    OUTER APPLY (
        SELECT 
            SUM(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END) as Balance,
            MAX(TransactionTime) as LastTransaction
        FROM [dbo].[Transaction] t
        WHERE t.AccountId = ti.InstrumentId
    ) t;
END
GO

-- Drop functions used by computed columns
DROP FUNCTION [dbo].[AccountBalance];
DROP FUNCTION [dbo].[LastTransaction];
GO
