CREATE TABLE [dbo].[AccountHolder]
(
    [AccountHolderId] UNIQUEIDENTIFIER NOT NULL,
    [EmailAddress] NVARCHAR(255) NOT NULL,
    [FirstName] NVARCHAR(255) NULL,
    [LastName] NVARCHAR(255) NULL,
    CONSTRAINT PK_AccountHolderId PRIMARY KEY (AccountHolderId)
)

GO

CREATE UNIQUE INDEX [IX_AccountHolder_Email] ON [dbo].[AccountHolder] ([EmailAddress])
