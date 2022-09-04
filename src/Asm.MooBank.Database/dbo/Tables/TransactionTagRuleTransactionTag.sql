CREATE TABLE [dbo].[TransactionTagRuleTransactionTag]
(
	[TransactionTagRuleId] INT NOT NULL,
    [TransactionTagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionTagRuleTransactionTag] PRIMARY KEY CLUSTERED (TransactionTagRuleId, TransactionTagId),
    CONSTRAINT [FK_TransactionTagRuleTransactionTag_TransactionTagRule] FOREIGN KEY (TransactionTagRuleId) REFERENCES [TransactionTagRule](TransactionTagRuleId),
    CONSTRAINT [FK_TransactionTagRuleTransactionTag_TransactionTag] FOREIGN KEY (TransactionTagId) REFERENCES [TransactionTag](TransactionTagId),
)
