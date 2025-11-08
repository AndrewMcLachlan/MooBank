-- Split InstitutionAccount into LogicalAccount and InstitutionAccount
-- LogicalAccount represents the logical/virtual account concept
-- InstitutionAccount represents the physical account at an institution
-- This migration preserves all existing data and is IDEMPOTENT (can be run multiple times safely)

-- Check if migration has already completed
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LogicalAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
    AND EXISTS (SELECT 1 FROM dbo.[InstitutionAccount])
BEGIN
    PRINT 'Migration already completed. Exiting.'
    RETURN;
END
GO

-- Cleanup: Drop any orphaned default constraints from previous failed runs
DECLARE @dropConstraintSQL NVARCHAR(MAX)

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_InstitutionAccount_Id')
BEGIN
    PRINT 'Cleanup: Dropping orphaned DF_InstitutionAccount_Id constraint'
    SELECT @dropConstraintSQL = 'ALTER TABLE [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] DROP CONSTRAINT [DF_InstitutionAccount_Id]'
    FROM sys.default_constraints dc
    INNER JOIN sys.tables t ON dc.parent_object_id = t.object_id
    WHERE dc.name = 'DF_InstitutionAccount_Id'
    
    IF @dropConstraintSQL IS NOT NULL
        EXEC sp_executesql @dropConstraintSQL;
END

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_InstitutionAccount_LastUpdated')
BEGIN
    PRINT 'Cleanup: Dropping orphaned DF_InstitutionAccount_LastUpdated constraint'
    SELECT @dropConstraintSQL = 'ALTER TABLE [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] DROP CONSTRAINT [DF_InstitutionAccount_LastUpdated]'
    FROM sys.default_constraints dc
    INNER JOIN sys.tables t ON dc.parent_object_id = t.object_id
    WHERE dc.name = 'DF_InstitutionAccount_LastUpdated'
    
    IF @dropConstraintSQL IS NOT NULL
        EXEC sp_executesql @dropConstraintSQL;
END
GO

-- Step 1: Drop dependent constraints from other tables that reference InstitutionAccount
PRINT 'Step 1: Dropping constraints from other tables'

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ImportAccount_InstitutionAccount')
    ALTER TABLE [dbo].[ImportAccount] DROP CONSTRAINT FK_ImportAccount_InstitutionAccount;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_User_PrimaryAccount')
    ALTER TABLE [dbo].[User] DROP CONSTRAINT FK_User_PrimaryAccount;
GO

-- Step 2: Drop constraints on the old InstitutionAccount table itself (if it still exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Step 2: Dropping constraints on InstitutionAccount table'

    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InstitutionAccount_TransactionAccount')
        ALTER TABLE [dbo].[InstitutionAccount] DROP CONSTRAINT FK_InstitutionAccount_TransactionAccount;

    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InstitutionAccount_AccountType')
        ALTER TABLE [dbo].[InstitutionAccount] DROP CONSTRAINT FK_InstitutionAccount_AccountType;

    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InstitutionAccount_Institution')
        ALTER TABLE [dbo].[InstitutionAccount] DROP CONSTRAINT FK_InstitutionAccount_Institution;

    -- Drop primary key constraint
    IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_InstitutionAccount')
        ALTER TABLE [dbo].[InstitutionAccount] DROP CONSTRAINT PK_InstitutionAccount;

    -- Drop default constraints
    IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_InstitutionAccount_Id')
        ALTER TABLE [dbo].[InstitutionAccount] DROP CONSTRAINT DF_InstitutionAccount_Id;

    IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_InstitutionAccount_LastUpdated')
        ALTER TABLE [dbo].[InstitutionAccount] DROP CONSTRAINT DF_InstitutionAccount_LastUpdated;
END
GO

-- Step 3: Rename old InstitutionAccount to preserve data during migration (if not already renamed)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Step 3: Renaming old InstitutionAccount table'
    EXEC sp_rename 'dbo.InstitutionAccount', 'InstitutionAccount_Old';
END
GO

-- Step 4: Create new LogicalAccount table (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LogicalAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Step 4: Creating new LogicalAccount table'

    CREATE TABLE dbo.[LogicalAccount]
    (
        [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
        [AccountTypeId] INT NOT NULL,
        [IncludeInBudget] BIT NOT NULL CONSTRAINT DF_LogicalAccount_IncludeInBudget DEFAULT 0,
        [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_LogicalAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_LogicalAccount PRIMARY KEY CLUSTERED ([InstrumentId]),
        CONSTRAINT FK_LogicalAccount_TransactionAccount FOREIGN KEY ([InstrumentId]) REFERENCES [TransactionInstrument]([InstrumentId]),
        CONSTRAINT FK_LogicalAccount_AccountType FOREIGN KEY (AccountTypeId) REFERENCES [AccountType]([Id]),
    )
END
GO

-- Step 5: Migrate data from old InstitutionAccount to new LogicalAccount (if not already migrated)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LogicalAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
    AND NOT EXISTS (SELECT 1 FROM dbo.[LogicalAccount])
BEGIN
    PRINT 'Step 5: Migrating data to LogicalAccount'

    INSERT INTO dbo.[LogicalAccount] ([InstrumentId], [AccountTypeId], [IncludeInBudget], [LastUpdated])
    SELECT 
        [InstrumentId],
        [AccountTypeId],
        [IncludeInBudget],
        [LastUpdated]
    FROM dbo.[InstitutionAccount_Old];
END
GO

-- Step 6: Create new InstitutionAccount table with ImporterTypeId (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Step 6: Creating new InstitutionAccount table'

    CREATE TABLE dbo.[InstitutionAccount]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_InstitutionAccount_Id DEFAULT NEWID(),
        [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
        [InstitutionId] INT NOT NULL,
        [ImporterTypeId] INT NULL,
        [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstitutionAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT FK_InstitutionAccount_LogicalAccount FOREIGN KEY ([InstrumentId]) REFERENCES [LogicalAccount]([InstrumentId]),
        CONSTRAINT FK_InstitutionAccount_Institution FOREIGN KEY (InstitutionId) REFERENCES Institution(Id)
    )
END
GO

-- Step 7: Migrate data to new InstitutionAccount, merging ImporterTypeId from ImportAccount (if not already migrated)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND NOT EXISTS (SELECT 1 FROM dbo.[InstitutionAccount])
BEGIN
    PRINT 'Step 7: Migrating data to new InstitutionAccount'

    -- If InstitutionAccount_Old still exists, migrate from it
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        INSERT INTO dbo.[InstitutionAccount] ([Id], [InstrumentId], [InstitutionId], [ImporterTypeId], [LastUpdated])
        SELECT 
            NEWID(), -- Use NEWID() to generate a new Id
            ia_old.[InstrumentId],
            ia_old.[InstitutionId],
            imp.[ImporterTypeId],
            ia_old.[LastUpdated]
        FROM dbo.[InstitutionAccount_Old] ia_old
        LEFT JOIN dbo.[ImportAccount] imp ON ia_old.[InstrumentId] = imp.[AccountId];
    END
    -- Otherwise, LogicalAccount has the data we need - create one InstitutionAccount per LogicalAccount
    ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LogicalAccount' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        PRINT 'Note: Creating InstitutionAccount records from LogicalAccount (InstitutionAccount_Old was already cleaned up)'
        
        -- This is a recovery path - we need to create InstitutionAccount records
        -- Since we don't have the original InstitutionId, we'll need to use a default or handle this manually
        -- For now, we'll raise an error to alert that manual intervention is needed
        RAISERROR('ERROR: InstitutionAccount table is empty but InstitutionAccount_Old no longer exists. Manual data recovery needed. Original InstitutionId and ImporterTypeId data has been lost.', 16, 1);
    END
END
GO

-- Step 8: Recreate foreign key constraint on User table (if not already exists)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_User_PrimaryAccount')
    AND EXISTS (SELECT * FROM sys.tables WHERE name = 'LogicalAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Step 8: Recreating User.PrimaryAccountId foreign key'

    ALTER TABLE [dbo].[User]
    ADD CONSTRAINT FK_User_PrimaryAccount FOREIGN KEY (PrimaryAccountId) REFERENCES LogicalAccount([InstrumentId]);
END
GO

-- Step 9: Drop ImportAccount table (no longer needed)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Step 9: Dropping ImportAccount table'
    DROP TABLE [dbo].[ImportAccount];
END
GO

-- Step 10: Clean up old table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
    AND EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND EXISTS (SELECT 1 FROM dbo.[InstitutionAccount])
BEGIN
    PRINT 'Step 10: Dropping old InstitutionAccount_Old table'
    DROP TABLE dbo.[InstitutionAccount_Old];
END
GO

PRINT 'Migration completed successfully!'
GO