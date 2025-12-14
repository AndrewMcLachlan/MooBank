-- Add TransactionId to child tables
IF COL_LENGTH('dbo.TransactionSplitTag', 'TransactionId') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplitTag] ADD [TransactionId] UNIQUEIDENTIFIER NULL;
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'TransactionId') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplitOffset] ADD [TransactionId] UNIQUEIDENTIFIER NULL;
END
GO

-- Populate TransactionId
IF EXISTS (SELECT 1 FROM [dbo].[TransactionSplitTag] WHERE TransactionId IS NULL)
BEGIN
    UPDATE tst
    SET TransactionId = ts.TransactionId
    FROM [dbo].[TransactionSplitTag] tst
    JOIN [dbo].[TransactionSplit] ts ON tst.TransactionSplitId = ts.Id;
END

IF EXISTS (SELECT 1 FROM [dbo].[TransactionSplitOffset] WHERE TransactionId IS NULL)
BEGIN
    UPDATE tso
    SET TransactionId = ts.TransactionId
    FROM [dbo].[TransactionSplitOffset] tso
    JOIN [dbo].[TransactionSplit] ts ON tso.TransactionSplitId = ts.Id;
END
GO

-- Make TransactionId NOT NULL
IF COL_LENGTH('dbo.TransactionSplitTag', 'TransactionId') IS NOT NULL 
   AND COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitTag'), 'TransactionId', 'AllowsNull') = 1
BEGIN
    ALTER TABLE [dbo].[TransactionSplitTag] ALTER COLUMN [TransactionId] UNIQUEIDENTIFIER NOT NULL;
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'TransactionId') IS NOT NULL 
   AND COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitOffset'), 'TransactionId', 'AllowsNull') = 1
BEGIN
    ALTER TABLE [dbo].[TransactionSplitOffset] ALTER COLUMN [TransactionId] UNIQUEIDENTIFIER NOT NULL;
END
GO

-- Add Id (int) to TransactionSplit
IF COL_LENGTH('dbo.TransactionSplit', 'Id') IS NOT NULL 
   AND TYPE_NAME(COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplit'), 'Id', 'SystemTypeId')) = 'uniqueidentifier'
BEGIN
    IF COL_LENGTH('dbo.TransactionSplit', 'NewId') IS NULL
    BEGIN
        ALTER TABLE [dbo].[TransactionSplit] ADD [NewId] INT NULL;
    END
END
GO

-- Populate NewId
IF COL_LENGTH('dbo.TransactionSplit', 'NewId') IS NOT NULL
BEGIN
    WITH CTE AS (
        SELECT Id, ROW_NUMBER() OVER (PARTITION BY TransactionId ORDER BY Id) as RowNum
        FROM [dbo].[TransactionSplit]
        WHERE NewId IS NULL
    )
    UPDATE ts
    SET NewId = c.RowNum
    FROM [dbo].[TransactionSplit] ts
    JOIN CTE c ON ts.Id = c.Id;
END
GO

ALTER TABLE [dbo].[TransactionSplit] ALTER COLUMN [NewId] INT NOT NULL;
GO

-- Add TransactionSplitId (int) to child tables
IF COL_LENGTH('dbo.TransactionSplitTag', 'TransactionSplitId') IS NOT NULL 
   AND TYPE_NAME(COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitTag'), 'TransactionSplitId', 'SystemTypeId')) = 'uniqueidentifier'
BEGIN
    IF COL_LENGTH('dbo.TransactionSplitTag', 'NewTransactionSplitId') IS NULL
    BEGIN
        ALTER TABLE [dbo].[TransactionSplitTag] ADD [NewTransactionSplitId] INT NULL;
    END
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'TransactionSplitId') IS NOT NULL 
   AND TYPE_NAME(COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitOffset'), 'TransactionSplitId', 'SystemTypeId')) = 'uniqueidentifier'
BEGIN
    IF COL_LENGTH('dbo.TransactionSplitOffset', 'NewTransactionSplitId') IS NULL
    BEGIN
        ALTER TABLE [dbo].[TransactionSplitOffset] ADD [NewTransactionSplitId] INT NULL;
    END
END
GO

-- Populate NewTransactionSplitId
IF COL_LENGTH('dbo.TransactionSplitTag', 'NewTransactionSplitId') IS NOT NULL
BEGIN
    UPDATE tst
    SET NewTransactionSplitId = ts.NewId
    FROM [dbo].[TransactionSplitTag] tst
    JOIN [dbo].[TransactionSplit] ts ON tst.TransactionSplitId = ts.Id
    WHERE tst.NewTransactionSplitId IS NULL;
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'NewTransactionSplitId') IS NOT NULL
BEGIN
    UPDATE tso
    SET NewTransactionSplitId = ts.NewId
    FROM [dbo].[TransactionSplitOffset] tso
    JOIN [dbo].[TransactionSplit] ts ON tso.TransactionSplitId = ts.Id
    WHERE tso.NewTransactionSplitId IS NULL;
END
GO

IF COL_LENGTH('dbo.TransactionSplitTag', 'NewTransactionSplitId') IS NOT NULL 
   AND COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitTag'), 'NewTransactionSplitId', 'AllowsNull') = 1
BEGIN
    ALTER TABLE [dbo].[TransactionSplitTag] ALTER COLUMN [NewTransactionSplitId] INT NOT NULL;
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'NewTransactionSplitId') IS NOT NULL 
   AND COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitOffset'), 'NewTransactionSplitId', 'AllowsNull') = 1
BEGIN
    ALTER TABLE [dbo].[TransactionSplitOffset] ALTER COLUMN [NewTransactionSplitId] INT NOT NULL;
END
GO

-- Drop Constraints
IF OBJECT_ID('FK_TransactionSplitTag_TransactionSplit') IS NOT NULL ALTER TABLE [dbo].[TransactionSplitTag] DROP CONSTRAINT [FK_TransactionSplitTag_TransactionSplit];
IF OBJECT_ID('PK_TransactionSplitTag') IS NOT NULL ALTER TABLE [dbo].[TransactionSplitTag] DROP CONSTRAINT [PK_TransactionSplitTag];
IF OBJECT_ID('FK_TransactionSplitOffset_TransactionSplit') IS NOT NULL ALTER TABLE [dbo].[TransactionSplitOffset] DROP CONSTRAINT [FK_TransactionSplitOffset_TransactionSplit];
IF OBJECT_ID('PK_TransactionSplitOffset') IS NOT NULL ALTER TABLE [dbo].[TransactionSplitOffset] DROP CONSTRAINT [PK_TransactionSplitOffset];
IF OBJECT_ID('PK_TransactionSplit') IS NOT NULL ALTER TABLE [dbo].[TransactionSplit] DROP CONSTRAINT [PK_TransactionSplit];
IF OBJECT_ID('FK_TransactionSplit_Transaction') IS NOT NULL ALTER TABLE [dbo].[TransactionSplit] DROP CONSTRAINT [FK_TransactionSplit_Transaction];
IF OBJECT_ID('DF_TransactionSplit_Id') IS NOT NULL ALTER TABLE [dbo].[TransactionSplit] DROP CONSTRAINT [DF_TransactionSplit_Id];
IF OBJECT_ID('CK_TransactionSplit_Amount') IS NOT NULL ALTER TABLE [dbo].[TransactionSplit] DROP CONSTRAINT [CK_TransactionSplit_Amount];
GO

-- Drop old columns
IF COL_LENGTH('dbo.TransactionSplitTag', 'TransactionSplitId') IS NOT NULL 
   AND TYPE_NAME(COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitTag'), 'TransactionSplitId', 'SystemTypeId')) = 'uniqueidentifier'
BEGIN
    ALTER TABLE [dbo].[TransactionSplitTag] DROP COLUMN [TransactionSplitId];
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'TransactionSplitId') IS NOT NULL 
   AND TYPE_NAME(COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplitOffset'), 'TransactionSplitId', 'SystemTypeId')) = 'uniqueidentifier'
BEGIN
    ALTER TABLE [dbo].[TransactionSplitOffset] DROP COLUMN [TransactionSplitId];
END

IF COL_LENGTH('dbo.TransactionSplit', 'Id') IS NOT NULL 
   AND TYPE_NAME(COLUMNPROPERTY(OBJECT_ID('dbo.TransactionSplit'), 'Id', 'SystemTypeId')) = 'uniqueidentifier'
BEGIN
    ALTER TABLE [dbo].[TransactionSplit] DROP COLUMN [Id];
END
GO

-- Rename new columns
IF COL_LENGTH('dbo.TransactionSplit', 'NewId') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.TransactionSplit.NewId', 'Id', 'COLUMN';
END

IF COL_LENGTH('dbo.TransactionSplitTag', 'NewTransactionSplitId') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.TransactionSplitTag.NewTransactionSplitId', 'TransactionSplitId', 'COLUMN';
END

IF COL_LENGTH('dbo.TransactionSplitOffset', 'NewTransactionSplitId') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.TransactionSplitOffset.NewTransactionSplitId', 'TransactionSplitId', 'COLUMN';
END
GO

-- Recreate PKs and FKs
IF OBJECT_ID('PK_TransactionSplit') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplit] ADD CONSTRAINT [PK_TransactionSplit] PRIMARY KEY CLUSTERED (TransactionId, Id);
END

IF OBJECT_ID('FK_TransactionSplit_Transaction') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplit] ADD CONSTRAINT [FK_TransactionSplit_Transaction] FOREIGN KEY (TransactionId) REFERENCES [dbo].[Transaction] (TransactionId) ON DELETE CASCADE;
END

IF OBJECT_ID('PK_TransactionSplitTag') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplitTag] ADD CONSTRAINT [PK_TransactionSplitTag] PRIMARY KEY CLUSTERED (TransactionId, TransactionSplitId, TagId);
END

IF OBJECT_ID('FK_TransactionSplitTag_TransactionSplit') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplitTag] ADD CONSTRAINT [FK_TransactionSplitTag_TransactionSplit] FOREIGN KEY (TransactionId, TransactionSplitId) REFERENCES [dbo].[TransactionSplit] (TransactionId, Id) ON DELETE CASCADE;
END

IF OBJECT_ID('PK_TransactionSplitOffset') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplitOffset] ADD CONSTRAINT [PK_TransactionSplitOffset] PRIMARY KEY CLUSTERED (TransactionId, TransactionSplitId, OffsetTransactionId);
END

IF OBJECT_ID('FK_TransactionSplitOffset_TransactionSplit') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplitOffset] ADD CONSTRAINT [FK_TransactionSplitOffset_TransactionSplit] FOREIGN KEY (TransactionId, TransactionSplitId) REFERENCES [dbo].[TransactionSplit] (TransactionId, Id) ON DELETE CASCADE;
END

IF OBJECT_ID('CK_TransactionSplit_Amount') IS NULL
BEGIN
    ALTER TABLE [dbo].[TransactionSplit] ADD CONSTRAINT [CK_TransactionSplit_Amount] CHECK([dbo].[CheckSplitAmount](Id, TransactionId, Amount) = 1);
END
GO
