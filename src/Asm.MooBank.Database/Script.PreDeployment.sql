IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InstitutionAccount]') AND type in (N'U'))
DROP TABLE [dbo].[InstitutionAccount]
GO


CREATE TABLE dbo.[InstitutionAccount]
(
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [AccountTypeId] INT NOT NULL,
    [AccountControllerId] INT NOT NULL,
    [IncludeInPosition] BIT NOT NULL CONSTRAINT DF_InstiutionAccount_IncludeInPosition DEFAULT 0,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstiutionAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_InstitutionAccount_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
    CONSTRAINT FK_InstitutionAccount_AccountType FOREIGN KEY (AccountTypeId) REFERENCES AccountType(AccountTypeId),
    CONSTRAINT FK_InstitutionAccount_AccountController FOREIGN KEY (AccountControllerId) REFERENCES AccountController(AccountControllerId)
)

INSERT INTO InstitutionAccount (AccountId, AccountTypeId, AccountControllerId, IncludeInPosition, LastUpdated) SELECT AccountId, AccountTypeId, AccountControllerId, IncludeInPosition, LastUpdated FROM Account

ALTER TABLE Account DROP CONSTRAINT DF_AvailableBalance
ALTER TABLE Account DROP CONSTRAINT DF_Account_IncludeInPosition
ALTER TABLE Account DROP CONSTRAINT FK_Account_AccountType
ALTER TABLE Account DROP CONSTRAINT FK_Account_AccountController
ALTER TABLE Account DROP CONSTRAINT DF_UpdateVirtualAccount
ALTER TABLE Account DROP COLUMN AvailableBalance
ALTER TABLE Account DROP COLUMN IncludeInPosition
ALTER TABLE Account DROP COLUMN AccountTypeId
ALTER TABLE Account DROP COLUMN AccountControllerId
ALTER TABLE Account DROP COLUMN UpdateVirtualAccount
EXEC sp_rename 'Account.AccountBalance', 'Balance',  'COLUMN'
EXEC sp_rename 'DF_AccountBalance', 'DF_Account_Balance',  'OBJECT'
GO

ALTER TABLE [dbo].[RecurringTransaction] DROP CONSTRAINT [FK_RecurringTransaction_VirtualAccount_Source]
ALTER TABLE [dbo].[RecurringTransaction] DROP CONSTRAINT [FK_RecurringTransaction_VirtualAccount_Destination]

CREATE TABLE [dbo].[VirtualAccount2]
(
    [AccountId] UNIQUEIDENTIFIER CONSTRAINT DF_VirtualAccount_AccountId DEFAULT NEWID(),
    [InstitutionAccountId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_VirtualAccount2 PRIMARY KEY CLUSTERED ([AccountId]),
    CONSTRAINT FK_VirtualAccount_Account2 FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](AccountId),
    CONSTRAINT FK_VirtualAccount_InstitutionAccount FOREIGN KEY (InstitutionAccountId) REFERENCES [dbo].[InstitutionAccount](AccountId)
)

INSERT INTO Account (AccountId, Name, Description, Balance, LastUpdated) SELECT VirtualAccountId, NAme, Description, Balance, SYSUTCDATETIME() FROM VirtualAccount

INSERT INTO VirtualAccount2 (AccountId, InstitutionAccountId) SELECT VirtualAccountId, AccountId FROM VirtualAccount

DROP TABLE VirtualAccount

EXEC sp_rename 'VirtualAccount2', 'VirtualAccount'
EXEC sp_rename 'PK_VirtualAccount2', 'PK_VirtualAccount',  'OBJECT'
EXEC sp_rename 'FK_VirtualAccount_Account2', 'FK_VirtualAccount_Account',  'OBJECT'
GO

EXEC sp_rename 'RecurringTransaction.DestinationVirtualAccountId', 'VirtualAccountId',  'COLUMN'
ALTER TABLE RecurringTransaction DROP COLUMN SourceVirtualAccountId
ALTER TABLE RecurringTransaction ADD CONSTRAINT FK_RecurringTransaction_VirtualAccount FOREIGN KEY (VirtualAccountId) REFERENCES [dbo].[VirtualAccount]([AccountId])