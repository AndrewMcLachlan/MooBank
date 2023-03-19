CREATE TABLE [dbo].[VirtualAccount]
(
    [AccountId] UNIQUEIDENTIFIER CONSTRAINT DF_VirtualAccount_AccountId DEFAULT NEWID(),
    [InstitutionAccountId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_VirtualAccount PRIMARY KEY CLUSTERED ([AccountId]),
    CONSTRAINT FK_VirtualAccount_Account FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](AccountId),
    CONSTRAINT FK_VirtualAccount_InstitutionAccount FOREIGN KEY (InstitutionAccountId) REFERENCES [dbo].[InstitutionAccount](AccountId)
)
GO

