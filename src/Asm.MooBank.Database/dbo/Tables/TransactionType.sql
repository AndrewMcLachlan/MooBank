CREATE TABLE [dbo].[TransactionType]
(
    [TransactionTypeId] INT NOT NULL,
    [Description] VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TransactionType] PRIMARY KEY CLUSTERED ([TransactionTypeId]),
)
