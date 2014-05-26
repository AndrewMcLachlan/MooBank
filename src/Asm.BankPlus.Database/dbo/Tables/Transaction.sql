CREATE TABLE [dbo].[Transaction]
(
	[TransactionId] INT NOT NULL PRIMARY KEY IDENTITY, 
	[TransactionGroupId] UNIQUEIDENTIFIER NULL, 
    [VirtualAccountId] UNIQUEIDENTIFIER NOT NULL, 
    [TransactionTypeId] INT NOT NULL, 
    [Amount] DECIMAL(10, 2) NOT NULL, 
    [Description] VARCHAR(255) NULL, 
    [TransactionTime] DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
    CONSTRAINT [FK_Transaction_VirtualAccount] FOREIGN KEY ([VirtualAccountId]) REFERENCES [VirtualAccount]([VirtualAccountId]), 
    CONSTRAINT [FK_Transaction_TransactionType] FOREIGN KEY ([TransactionTypeId]) REFERENCES [TransactionType]([TransactionTypeId]) 
)

GO

CREATE TRIGGER [dbo].[Trigger_Transaction]
    ON [dbo].[Transaction]
    FOR INSERT
    AS
    BEGIN
        DECLARE @transactionTypeId int
		DECLARE @virtualAccountId uniqueidentifier
		DECLARE @amount decimal(10,2)
		
		SELECT @transactionTypeId = TransactionTypeId, @virtualAccountId = VirtualAccountId, @amount = Amount FROM inserted

		IF (@transactionTypeId = 1 OR @transactionTypeId = 3)
			UPDATE VirtualAccount SET Balance = Balance + @amount WHERE VirtualAccountId = @virtualAccountId
		
		ELSE IF (@transactionTypeId = 2 OR @transactionTypeId = 4)
			UPDATE VirtualAccount SET Balance = Balance - @amount WHERE VirtualAccountId = @virtualAccountId
		
		ELSE IF (@transactionTypeId = 5)
			UPDATE VirtualAccount SET Balance = @amount WHERE VirtualAccountId = @virtualAccountId
    END