CREATE TABLE [dbo].[CardOwner]
(
	[CardOwnerId] INT NOT NULL IDENTITY(1,1),
    [CardNumber] VARCHAR(16) NOT NULL,
    [AccountHolderId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_CardOwner PRIMARY KEY CLUSTERED (CardOwnerId),
    CONSTRAINT FK_CardOwner_AccountHolder FOREIGN KEY (AccountHolderId) REFERENCES [User]([Id]),
)
