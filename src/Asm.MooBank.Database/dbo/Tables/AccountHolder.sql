CREATE TABLE [dbo].[AccountHolder]
(
    [AccountHolderId] UNIQUEIDENTIFIER NOT NULL,
    [EmailAddress] NVARCHAR(255) NOT NULL,
    [FirstName] NVARCHAR(255) NULL,
    [LastName] NVARCHAR(255) NULL,
    [Currency] CHAR(3) NOT NULL CONSTRAINT DF_AccountHolder_Currency DEFAULT 'AUD',
    [FamilyId] UNIQUEIDENTIFIER NULL,
    [PrimaryAccountId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_AccountHolderId PRIMARY KEY (AccountHolderId),
    CONSTRAINT FK_AccountHolder_Family FOREIGN KEY (FamilyId) REFERENCES Family(Id),
    CONSTRAINT FK_AccountHolder_PrimaryAccount FOREIGN KEY (PrimaryAccountId) REFERENCES InstitutionAccount(AccountId),
)

GO

CREATE UNIQUE INDEX [IX_AccountHolder_Email] ON [dbo].[AccountHolder] ([EmailAddress])
