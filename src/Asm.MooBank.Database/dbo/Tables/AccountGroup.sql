CREATE TABLE AccountGroup (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AccountGroup_Id DEFAULT (NEWID()),
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
    [ShowPosition] BIT NOT NULL,

    CONSTRAINT PK_AccountGroup PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_AccountGroup_AccountHolder FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[AccountHolder](AccountHolderId)
)