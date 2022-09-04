CREATE TABLE [dbo].[TransactionTagRule]
(
	[TransactionTagRuleId] INT NOT NULL IDENTITY(1,1),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Contains] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TransactionTagRule] PRIMARY KEY CLUSTERED (TransactionTagRuleId),
    CONSTRAINT [FK_TransactionTagRule_Account] FOREIGN KEY (AccountId) REFERENCEs [Account](AccountId),
)
