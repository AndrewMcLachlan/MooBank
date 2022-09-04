CREATE TABLE [dbo].[ImporterType]
(
	[ImporterTypeId] INT NOT NULL IDENTITY(1,1),
    [Type] NVARCHAR(255) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_ImporterType] PRIMARY KEY CLUSTERED (ImporterTypeId)
)
GO

CREATE UNIQUE INDEX [IX_ImporterType] ON [dbo].[ImporterType]([Type])
GO
