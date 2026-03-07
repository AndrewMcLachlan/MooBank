-- Rollback script for LogicalAccount migration
-- This will restore the original InstitutionAccount structure
-- WARNING: This assumes you have a backup or the original data is still available

PRINT 'Rolling back LogicalAccount migration...'

-- Step 1: Drop the FK from User to LogicalAccount
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_User_PrimaryAccount' AND parent_object_id = OBJECT_ID('dbo.User'))
BEGIN
    PRINT 'Dropping FK_User_PrimaryAccount'
    ALTER TABLE [dbo].[User] DROP CONSTRAINT FK_User_PrimaryAccount;
END
GO

-- Step 2: Drop the new InstitutionAccount table (if it exists and is empty or has wrong structure)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
    AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InstitutionAccount') AND name = 'Id')
BEGIN
    PRINT 'Dropping new InstitutionAccount table'
    DROP TABLE [dbo].[InstitutionAccount];
END
GO

-- Step 3: Drop LogicalAccount table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LogicalAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Dropping LogicalAccount table'
    DROP TABLE [dbo].[LogicalAccount];
END
GO

-- Step 4: Restore from backup or InstitutionAccount_Old if it still exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount_Old' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Restoring InstitutionAccount from InstitutionAccount_Old'
    EXEC sp_rename 'dbo.InstitutionAccount_Old', 'InstitutionAccount';
    
    -- Recreate the original constraints (you'll need to adjust these based on your actual schema)
    ALTER TABLE [dbo].[InstitutionAccount]
    ADD CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED ([InstrumentId]);
    
    -- Add back foreign keys - adjust these based on your original schema
    ALTER TABLE [dbo].[InstitutionAccount]
    ADD CONSTRAINT FK_InstitutionAccount_TransactionAccount FOREIGN KEY ([InstrumentId]) REFERENCES [TransactionInstrument]([InstrumentId]);
    
    ALTER TABLE [dbo].[InstitutionAccount]
    ADD CONSTRAINT FK_InstitutionAccount_AccountType FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountType]([Id]);
    
    ALTER TABLE [dbo].[InstitutionAccount]
    ADD CONSTRAINT FK_InstitutionAccount_Institution FOREIGN KEY ([InstitutionId]) REFERENCES [Institution]([Id]);
END
ELSE
BEGIN
    PRINT 'ERROR: InstitutionAccount_Old does not exist. You will need to restore from a database backup.'
    RAISERROR('Cannot rollback - InstitutionAccount_Old table not found. Restore from backup required.', 16, 1);
END
GO

-- Step 5: Recreate ImportAccount table (if needed)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Recreating ImportAccount table - NOTE: Data will need to be restored from backup'
    
    CREATE TABLE [dbo].[ImportAccount]
    (
        AccountId UNIQUEIDENTIFIER NOT NULL,
        ImporterTypeId INT NOT NULL,
        CONSTRAINT PK_ImportAccount PRIMARY KEY CLUSTERED (AccountId),
        CONSTRAINT FK_ImportAccount_InstitutionAccount FOREIGN KEY (AccountId) REFERENCES [dbo].[InstitutionAccount]([InstrumentId]),
        CONSTRAINT FK_ImportAccount_ImporterType FOREIGN KEY (ImporterTypeId) REFERENCES [dbo].[ImporterType](ImporterTypeId)
    )
    
    PRINT 'WARNING: ImportAccount table created but empty - restore data from backup'
END
GO

-- Step 6: Restore FK from User back to InstitutionAccount
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_User_PrimaryAccount')
    AND EXISTS (SELECT * FROM sys.tables WHERE name = 'InstitutionAccount' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Restoring FK_User_PrimaryAccount to InstitutionAccount'
    ALTER TABLE [dbo].[User]
    ADD CONSTRAINT FK_User_PrimaryAccount FOREIGN KEY (PrimaryAccountId) REFERENCES [dbo].[InstitutionAccount]([InstrumentId]);
END
GO

PRINT 'Rollback completed. If InstitutionAccount_Old did not exist, you MUST restore from backup.'
GO
