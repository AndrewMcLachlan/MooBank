CREATE TABLE [dbo].[VirtualAccount]
(
    [AccountId] UNIQUEIDENTIFIER CONSTRAINT DF_VirtualAccount_AccountId DEFAULT NEWID(),
    [ParentAccountId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_VirtualAccount PRIMARY KEY CLUSTERED ([AccountId]),
    CONSTRAINT FK_VirtualAccount_Account FOREIGN KEY (AccountId) REFERENCES [dbo].[Instrument]([Id]),
    CONSTRAINT FK_VirtualAccount_Account_Parent FOREIGN KEY (ParentAccountId) REFERENCES [dbo].[Instrument]([Id])
)
GO

