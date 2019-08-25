CREATE TABLE [dbo].[TransactionCategory]
(
	[TransactionCategoryId] INT NOT NULL IDENTITY(1,1),
    [Description] NVARCHAR(255) NOT NULL,
    [IsLivingExpense] BIT NOT NULL,
    [ParentCategoryId] INT NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Deleted] DEFAULT 0,
    CONSTRAINT [PK_TransactionCategory] PRIMARY KEY CLUSTERED (TransactionCategoryId),
    CONSTRAINT [FK_TransactionCategory_TransactionCategory] FOREIGN KEY (ParentCategoryId) REFERENCES TransactionCategory(TransactionCategoryId),
)
