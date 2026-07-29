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

/*
 Retirement plan members referenced a person by a free-text Name, which is being replaced by a
 UserId. Existing rows are matched to a user through the superannuation accounts already linked to
 them: a member's accounts are owned by exactly one person, so the pairing is derived from recorded
 ownership rather than guessed. The column is added and backfilled here so the main deployment can
 make it NOT NULL.

 A member with no accounts, or with accounts owned by more than one person, cannot be resolved and
 is left null — the deployment will then fail loudly on the NOT NULL rather than invent an identity.
*/
IF OBJECT_ID('dbo.RetirementPlanMember', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.RetirementPlanMember', 'UserId') IS NULL
BEGIN
    ALTER TABLE dbo.RetirementPlanMember ADD [UserId] UNIQUEIDENTIFIER NULL;

    EXEC ('
        UPDATE m
        SET m.UserId = owners.UserId
        FROM dbo.RetirementPlanMember m
        CROSS APPLY (
            SELECT MIN(o.UserId) AS UserId
            FROM dbo.RetirementPlanMemberAccount a
            JOIN dbo.InstrumentOwner o ON o.InstrumentId = a.InstrumentId
            WHERE a.RetirementPlanMemberId = m.Id
            HAVING COUNT(DISTINCT o.UserId) = 1
        ) owners;
    ');
END
GO
