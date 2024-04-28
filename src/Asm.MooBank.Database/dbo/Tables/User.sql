CREATE TABLE [dbo].[User]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [EmailAddress] NVARCHAR(255) NOT NULL,
    [FirstName] NVARCHAR(255) NULL,
    [LastName] NVARCHAR(255) NULL,
    [Currency] CHAR(3) NOT NULL CONSTRAINT DF_AccountHolder_Currency DEFAULT 'AUD',
    [FamilyId] UNIQUEIDENTIFIER NOT NULL,
    [PrimaryAccountId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_User PRIMARY KEY ([Id]),
    CONSTRAINT FK_User_Family FOREIGN KEY (FamilyId) REFERENCES Family(Id),
    CONSTRAINT FK_User_PrimaryAccount FOREIGN KEY (PrimaryAccountId) REFERENCES InstitutionAccount([InstrumentId]),
)

GO

CREATE UNIQUE INDEX [IX_User_Email] ON [dbo].[User] ([EmailAddress])
