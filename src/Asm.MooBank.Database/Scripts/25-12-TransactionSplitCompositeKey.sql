-- Add TransactionId to child tables
ALTER TABLE [dbo].[TransactionSplitTag] ADD [TransactionId] UNIQUEIDENTIFIER NULL;
ALTER TABLE [dbo].[TransactionSplitOffset] ADD [TransactionId] UNIQUEIDENTIFIER NULL;
GO

-- Populate TransactionId
UPDATE tst
SET TransactionId = ts.TransactionId
FROM [dbo].[TransactionSplitTag] tst
JOIN [dbo].[TransactionSplit] ts ON tst.TransactionSplitId = ts.Id;

UPDATE tso
SET TransactionId = ts.TransactionId
FROM [dbo].[TransactionSplitOffset] tso
JOIN [dbo].[TransactionSplit] ts ON tso.TransactionSplitId = ts.Id;
GO

-- Make TransactionId NOT NULL
ALTER TABLE [dbo].[TransactionSplitTag] ALTER COLUMN [TransactionId] UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE [dbo].[TransactionSplitOffset] ALTER COLUMN [TransactionId] UNIQUEIDENTIFIER NOT NULL;
GO

-- Add Id (int) to TransactionSplit
ALTER TABLE [dbo].[TransactionSplit] ADD [NewId] INT NULL;
GO

-- Populate NewId
WITH CTE AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY TransactionId ORDER BY Id) as RowNum
    FROM [dbo].[TransactionSplit]
)
UPDATE ts
SET NewId = c.RowNum
FROM [dbo].[TransactionSplit] ts
JOIN CTE c ON ts.Id = c.Id;
GO

ALTER TABLE [dbo].[TransactionSplit] ALTER COLUMN [NewId] INT NOT NULL;
GO

-- Add TransactionSplitId (int) to child tables
ALTER TABLE [dbo].[TransactionSplitTag] ADD [NewTransactionSplitId] INT NULL;
ALTER TABLE [dbo].[TransactionSplitOffset] ADD [NewTransactionSplitId] INT NULL;
GO

-- Populate NewTransactionSplitId
UPDATE tst
SET NewTransactionSplitId = ts.NewId
FROM [dbo].[TransactionSplitTag] tst
JOIN [dbo].[TransactionSplit] ts ON tst.TransactionSplitId = ts.Id;

UPDATE tso
SET NewTransactionSplitId = ts.NewId
FROM [dbo].[TransactionSplitOffset] tso
JOIN [dbo].[TransactionSplit] ts ON tso.TransactionSplitId = ts.Id;
GO

ALTER TABLE [dbo].[TransactionSplitTag] ALTER COLUMN [NewTransactionSplitId] INT NOT NULL;
ALTER TABLE [dbo].[TransactionSplitOffset] ALTER COLUMN [NewTransactionSplitId] INT NOT NULL;
GO

-- Drop Constraints
ALTER TABLE [dbo].[TransactionSplitTag] DROP CONSTRAINT [FK_TransactionSplitTag_TransactionSplit];
ALTER TABLE [dbo].[TransactionSplitTag] DROP CONSTRAINT [PK_TransactionSplitTag];
ALTER TABLE [dbo].[TransactionSplitOffset] DROP CONSTRAINT [FK_TransactionSplitOffset_TransactionSplit];
ALTER TABLE [dbo].[TransactionSplitOffset] DROP CONSTRAINT [PK_TransactionSplitOffset];
ALTER TABLE [dbo].[TransactionSplit] DROP CONSTRAINT [PK_TransactionSplit];
ALTER TABLE [dbo].[TransactionSplit] DROP CONSTRAINT [FK_TransactionSplit_Transaction];
GO

-- Drop old columns
ALTER TABLE [dbo].[TransactionSplitTag] DROP COLUMN [TransactionSplitId];
ALTER TABLE [dbo].[TransactionSplitOffset] DROP COLUMN [TransactionSplitId];
ALTER TABLE [dbo].[TransactionSplit] DROP COLUMN [Id];
GO

-- Rename new columns
EXEC sp_rename 'dbo.TransactionSplit.NewId', 'Id', 'COLUMN';
EXEC sp_rename 'dbo.TransactionSplitTag.NewTransactionSplitId', 'TransactionSplitId', 'COLUMN';
EXEC sp_rename 'dbo.TransactionSplitOffset.NewTransactionSplitId', 'TransactionSplitId', 'COLUMN';
GO

-- Recreate PKs and FKs
ALTER TABLE [dbo].[TransactionSplit] ADD CONSTRAINT [PK_TransactionSplit] PRIMARY KEY CLUSTERED (TransactionId, Id);
ALTER TABLE [dbo].[TransactionSplit] ADD CONSTRAINT [FK_TransactionSplit_Transaction] FOREIGN KEY (TransactionId) REFERENCES [dbo].[Transaction] (TransactionId) ON DELETE CASCADE;

ALTER TABLE [dbo].[TransactionSplitTag] ADD CONSTRAINT [PK_TransactionSplitTag] PRIMARY KEY CLUSTERED (TransactionId, TransactionSplitId, TagId);
ALTER TABLE [dbo].[TransactionSplitTag] ADD CONSTRAINT [FK_TransactionSplitTag_TransactionSplit] FOREIGN KEY (TransactionId, TransactionSplitId) REFERENCES [dbo].[TransactionSplit] (TransactionId, Id) ON DELETE CASCADE;

ALTER TABLE [dbo].[TransactionSplitOffset] ADD CONSTRAINT [PK_TransactionSplitOffset] PRIMARY KEY CLUSTERED (TransactionId, TransactionSplitId, OffsetTransactionId);
ALTER TABLE [dbo].[TransactionSplitOffset] ADD CONSTRAINT [FK_TransactionSplitOffset_TransactionSplit] FOREIGN KEY (TransactionId, TransactionSplitId) REFERENCES [dbo].[TransactionSplit] (TransactionId, Id) ON DELETE CASCADE;
GO
