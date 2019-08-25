CREATE TABLE [dbo].[AccountHolder]
(
	[AccountHolderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AccountHolderId DEFAULT (NEWID()),
    [EmailAddress] NVARCHAR(255) NOT NULL,
    [FirstName] NVARCHAR(255) NOT NULL,
    [LastName] NVARCHAR(255) NOT NULL,
    CONSTRAINT PK_AccountHolderId PRIMARY KEY (AccountHolderId)
)

GO

CREATE UNIQUE INDEX [IX_AccountHolder_Email] ON [dbo].[AccountHolder] ([EmailAddress]) 
