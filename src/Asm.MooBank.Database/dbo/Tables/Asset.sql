CREATE TABLE [dbo].[Asset]
(
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [PurchasePrice] DECIMAL(12,4) NOT NULL,
    CONSTRAINT PK_Asset PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_Asset_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
)
