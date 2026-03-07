CREATE TABLE [dbo].[RuleTag]
(
    [RuleId] INT NOT NULL,
    [TagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionTagRuleTransactionTag] PRIMARY KEY CLUSTERED ([RuleId], [TagId]),
    CONSTRAINT [FK_RuleTag_Rule] FOREIGN KEY ([RuleId]) REFERENCES [Rule]([Id]),
    CONSTRAINT [FK_RuleTag_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id]),
)
