CREATE TABLE [dbo].[TransactionTag]
(
	[TransactionTagId] INT NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(50) NOT NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Deleted] DEFAULT 0,
    CONSTRAINT [PK_TransactionTag] PRIMARY KEY CLUSTERED (TransactionTagId),
)
