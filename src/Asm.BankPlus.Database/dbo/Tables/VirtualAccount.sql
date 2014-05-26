CREATE TABLE [dbo].[VirtualAccount]
(
	[VirtualAccountId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(), 
    [Name] VARCHAR(50) NOT NULL, 
    [Description] VARCHAR(MAX) NULL, 
    [Balance] DECIMAL(10, 2) NOT NULL DEFAULT 0, 
    [DefaultAccount] BIT NOT NULL DEFAULT 0,
	[Closed] BIT NOT NULL DEFAULT 0,
)

GO

CREATE UNIQUE INDEX [IX_VirtualAccount_DefaultAccount] ON [dbo].[VirtualAccount] ([DefaultAccount]) WHERE DefaultAccount = 1
