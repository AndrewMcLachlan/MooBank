CREATE TABLE [dbo].[VirtualAccount]
(
	[AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Name] VARCHAR(50) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [Balance] DECIMAL(10, 2) NOT NULL DEFAULT 0,
    [DefaultAccount] BIT NOT NULL DEFAULT 0,
	[Closed] BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_VirtualAccount PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_VirtualAccount_Account FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](AccountId)
)

GO

CREATE UNIQUE INDEX [IX_VirtualAccount_DefaultAccount] ON [dbo].[VirtualAccount] ([DefaultAccount]) WHERE DefaultAccount = 1
