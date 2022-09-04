CREATE TABLE [dbo].[Transaction]
(
	[TransactionId] UNIQUEIDENTIFIER CONSTRAINT DF_TransactionId DEFAULT NEWID(),
	[TransactionReference] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionTypeId] INT NULL,
    [Amount] DECIMAL(10, 2) NOT NULL,
    [Description] VARCHAR(255) NULL,
    [TransactionTime] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_Transaction] PRIMARY KEY CLUSTERED (TransactionId),
    CONSTRAINT [FK_Transaction_Account] FOREIGN KEY ([AccountId]) REFERENCES [Account]([AccountId]),
    CONSTRAINT [FK_Transaction_TransactionType] FOREIGN KEY ([TransactionTypeId]) REFERENCES [TransactionType]([TransactionTypeId]),
)

GO

/*CREATE TRIGGER [dbo].[Trigger_Transaction]
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
    END*/