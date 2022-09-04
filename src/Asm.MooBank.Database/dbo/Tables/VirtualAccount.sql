CREATE TABLE [dbo].[VirtualAccount]
(
    [VirtualAccountId] UNIQUEIDENTIFIER CONSTRAINT DF_VirtualAccount_Id DEFAULT NEWID(),
	[AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Name] VARCHAR(50) NOT NULL,
    [Description] VARCHAR(255) NULL,
    [Balance] DECIMAL(10, 2) NOT NULL CONSTRAINT DF_VirtualAccount_Balance DEFAULT 0,
    CONSTRAINT PK_VirtualAccount PRIMARY KEY CLUSTERED ([VirtualAccountId]),
    CONSTRAINT FK_VirtualAccount_Account FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](AccountId)
)
GO

