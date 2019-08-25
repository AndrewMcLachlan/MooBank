CREATE TABLE [dbo].[AccountType]
(
	[AccountTypeId] INT NOT NULL,
    [Description] NVARCHAR(255),
    CONSTRAINT PK_AccountTypeId PRIMARY KEY CLUSTERED (AccountTypeId)
)
