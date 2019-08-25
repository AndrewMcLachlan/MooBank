CREATE TABLE [dbo].[TransactionCategoryRule]
(
	[TransactionCategoryRuleId] INT NOT NULL IDENTITY(1,1),
    [Contains] NVARCHAR(50) NOT NULL,
    [TransactionCategoryId] INT NOT NULL,
    CONSTRAINT [PK_TransactionCategoryRule] PRIMARY KEY CLUSTERED (TransactionCategoryRuleId),
    CONSTRAINT [FK_TransactionCategoryRule_TransactionCategory] FOREIGN KEY (TransactionCategoryId) REFERENCES TransactionCategory(TransactionCategoryId)
)
