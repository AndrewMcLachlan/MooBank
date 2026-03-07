CREATE TABLE [dbo].[UserCard]
(
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(50) NULL,
    [Last4Digits] SMALLINT NOT NULL,
    CONSTRAINT PK_UserCard PRIMARY KEY CLUSTERED ([UserId], Last4Digits),
    CONSTRAINT FK_UserCard_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id])
)