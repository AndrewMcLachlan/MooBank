CREATE TABLE [dbo].[TagTag]
(
    [PrimaryTagId] INT NOT NULL,
    [SecondaryTagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionTagTransactionTag] PRIMARY KEY CLUSTERED ([PrimaryTagId], [SecondaryTagId]),
    CONSTRAINT [FK_TagTag_Primary] FOREIGN KEY ([PrimaryTagId]) REFERENCES [Tag]([Id]),
    CONSTRAINT [FK_TagTag_Secondary] FOREIGN KEY ([SecondaryTagId]) REFERENCES [Tag]([Id]),
)
