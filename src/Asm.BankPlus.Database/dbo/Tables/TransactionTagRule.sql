CREATE TABLE [dbo].[TransactionTagRule]
(
	[TransactionTagRuleId] INT NOT NULL IDENTITY(1,1),
    [Contains] NVARCHAR(50) NOT NULL,
    [TransactionTagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionTagRule] PRIMARY KEY CLUSTERED (TransactionTagRuleId),
    CONSTRAINT [FK_TransactionTagRule_TransactionTag] FOREIGN KEY (TransactionTagId) REFERENCES TransactionTag(TransactionTagId)
)
