﻿CREATE TABLE [dbo].[Instrument]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AccountId DEFAULT (NEWID()),
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(255) NULL,
    [Currency] CHAR(3) NOT NULL CONSTRAINT DF_Account_Currency DEFAULT 'AUD',
    [Balance] DECIMAL(12, 4) NOT NULL CONSTRAINT DF_AccountBalance DEFAULT 0,
    [ControllerId] INT NULL,
    [ShareWithFamily] BIT NOT NULL CONSTRAINT DF_Account_ShareWithFamily DEFAULT 0,
    [Slug] VARCHAR(50) NULL,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Account PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT FK_Instrument_Controller FOREIGN KEY (ControllerId) REFERENCES [Controller]([Id]),
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
