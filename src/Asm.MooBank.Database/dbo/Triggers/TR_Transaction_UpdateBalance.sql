CREATE TRIGGER [dbo].[TR_Transaction_UpdateBalance]
ON [dbo].[Transaction]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BalanceChanges TABLE (AccountId UNIQUEIDENTIFIER PRIMARY KEY, NetChange DECIMAL(12, 4));

    -- Calculate the net change to apply to the balance (Delta Update)
    -- This is O(1) complexity regardless of account history size
    INSERT INTO @BalanceChanges (AccountId, NetChange)
    SELECT 
        AccountId, 
        SUM(AmountSigned)
    FROM (
        -- New values (Add to balance)
        SELECT 
            AccountId, 
            CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END as AmountSigned
        FROM inserted
        WHERE AccountId IS NOT NULL
        
        UNION ALL
        
        -- Old values (Subtract from balance)
        SELECT 
            AccountId, 
            -(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END) as AmountSigned
        FROM deleted
        WHERE AccountId IS NOT NULL
    ) d
    GROUP BY AccountId;

    -- Apply changes
    UPDATE ti
    SET 
        Balance = ISNULL(ti.Balance, 0) + bc.NetChange,
        -- Finding MAX on a Clustered Index (AccountId, TransactionTime) is extremely fast (Seek)
        LastTransaction = (
            SELECT MAX(TransactionTime) 
            FROM [dbo].[Transaction] t 
            WHERE t.AccountId = ti.InstrumentId
        )
    FROM [dbo].[TransactionInstrument] ti
    INNER JOIN @BalanceChanges bc ON ti.InstrumentId = bc.AccountId;
END
