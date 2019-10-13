CREATE TABLE [dbo].[ImportAccount]
(
    AccountId UNIQUEIDENTIFIER NOT NULL,
    ImporterTypeId INT NOT NULL,
    CONSTRAINT PK_ImportAccount PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_ImportAccount_Account FOREIGN KEY (AccountId) REFERENCES [dbo].[Account](AccountId),
    CONSTRAINT FK_ImportAccount_ImporterType FOREIGN KEY (ImporterTypeId) REFERENCES [dbo].[ImporterType](ImporterTypeId)
)
