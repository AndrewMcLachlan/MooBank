CREATE TABLE [dbo].[UserCard]
(
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(50) NULL,
    [Last4Digits] SMALLINT NOT NULL,
    CONSTRAINT PK_AccountHolderCard PRIMARY KEY CLUSTERED ([UserId], Last4Digits),
    CONSTRAINT FK_AccountHolderCard_AccountHolder FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id])
)