CREATE TABLE [dbo].[AccountController]
(
	[AccountControllerId] INT NOT NULL,
    [Type] NVARCHAR(255) NOT NULL,
    CONSTRAINT PK_AccountController PRIMARY KEY CLUSTERED (AccountControllerId),
)
