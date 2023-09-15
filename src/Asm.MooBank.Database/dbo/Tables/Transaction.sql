CREATE TABLE [dbo].[Transaction]
(
    [TransactionId] UNIQUEIDENTIFIER CONSTRAINT DF_TransactionId DEFAULT NEWID(),
    [TransactionReference] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionTypeId] INT NULL,
    [Amount] DECIMAL(10, 2) NOT NULL,
    [NetAmount] AS dbo.TransactionNetAmount(TransactionId, Amount),
    [Description] VARCHAR(255) NULL,
    [TransactionTime] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [Notes] NVARCHAR(512) NULL,
    [ExcludeFromReporting] BIT NOT NULL CONSTRAINT DF_Transaction_ExcludeFromReporting DEFAULT(0),
    [OffsetByTransactionId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Transaction] PRIMARY KEY CLUSTERED (TransactionId),
    CONSTRAINT [FK_Transaction_Account] FOREIGN KEY ([AccountId]) REFERENCES [Account]([AccountId]),
    CONSTRAINT [FK_Transaction_TransactionType] FOREIGN KEY ([TransactionTypeId]) REFERENCES [TransactionType]([TransactionTypeId]),
    CONSTRAINT [FK_Transaction_Transaction] FOREIGN KEY ([TransactionId]) REFERENCES [Transaction]([TransactionId]),
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