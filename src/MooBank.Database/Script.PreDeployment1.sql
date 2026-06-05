/*
 Pre-Deployment Script
 Stashes the existing Institution → ImporterType mapping (from InstitutionAccount.ImporterTypeId)
 into a staging table before the schema changes drop the column. The post-deployment script
 applies the mapping to the new Institution.ImporterTypeId column and removes the staging table.
*/

IF OBJECT_ID('dbo.InstitutionAccount', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.InstitutionAccount', 'ImporterTypeId') IS NOT NULL
BEGIN
    IF OBJECT_ID('dbo.__InstitutionImporterMigration', 'U') IS NOT NULL
        DROP TABLE dbo.__InstitutionImporterMigration;

    -- Wrapped in EXEC so the column reference is bound at runtime; otherwise
    -- the batch fails to parse on any deploy where ImporterTypeId no longer exists.
    EXEC ('
        SELECT InstitutionId, MAX(ImporterTypeId) AS ImporterTypeId
        INTO dbo.__InstitutionImporterMigration
        FROM dbo.InstitutionAccount
        WHERE ImporterTypeId IS NOT NULL
        GROUP BY InstitutionId;
    ');
END
