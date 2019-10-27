CREATE TABLE [dbo].[Account]
(
    [AccountId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AccountId DEFAULT (NEWID()),
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(255) NULL,
    [AccountBalance] DECIMAL(10, 2) NOT NULL CONSTRAINT DF_AccountBalance DEFAULT 0,
    [AvailableBalance] DECIMAL(10, 2) NOT NULL CONSTRAINT DF_AvailableBalance DEFAULT 0,
    [IncludeInPosition] BIT NOT NULL CONSTRAINT DF_Account_IncludeInPosition DEFAULT 0,
    [AccountTypeId] INT NOT NULL,
    [AccountControllerId] INT NOT NULL,
    [UpdateVirtualAccount] BIT NOT NULL CONSTRAINT DF_UpdateVirtualAccount DEFAULT 0,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Account PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_Account_AccountType FOREIGN KEY (AccountTypeId) REFERENCES AccountType(AccountTypeId),
    CONSTRAINT FK_Account_AccountController FOREIGN KEY (AccountControllerId) REFERENCES AccountController(AccountControllerId)
)

GO

/*CREATE TRIGGER [dbo].[Trigger_Account]
    ON [dbo].[Account]
    FOR INSERT, UPDATE
    AS
    BEGIN

        DECLARE @newId uniqueidentifier
        DECLARE @newBalance decimal(10,2)
        DECLARE @newUpdateVirtualAccount bit
        DECLARE @defaultAccount uniqueidentifier
        DECLARE @newName varchar(50)

        SELECT @newUpdateVirtualAccount = UpdateVirtualAccount FROM inserted

        IF (@newUpdateVirtualAccount = 1)
        BEGIN
            SELECT @newId = AccountId, @newName = Name, @newBalance = [AccountBalance] FROM inserted

            DECLARE @virtualbalance decimal(10,2)
            SELECT @virtualbalance = SUM(Balance) FROM VirtualAccount va WHERE va.DefaultAccount = 0

            DECLARE @balance decimal(10,2)
            SELECT @balance = SUM([AccountBalance]) + @newBalance FROM Account a WHERE a.AccountId <> @newId AND UpdateVirtualAccount = 1


            SELECT @defaultAccount = VirtualAccountId FROM VirtualAccount WHERE DefaultAccount = 1
            INSERT INTO [Transaction] (VirtualAccountId, TransactionTypeId, Amount, [Description]) VALUES (@defaultAccount, 5, ISNULL(@balance, @newBalance)-ISNULL(@virtualbalance,0), 'Balance change to ' + @newName)

        END
    END
GO*/

CREATE UNIQUE INDEX [IX_Account_Name] ON [dbo].[Account] ([Name])
