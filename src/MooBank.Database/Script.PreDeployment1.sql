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
 ownership rather than guessed.

 The rows are moved out to staging tables here and put back by the post-deployment script, rather
 than backfilled in place. Adding UserId to the live table does not survive: dropping Name forces
 SSDT to rebuild RetirementPlanMember, and the rebuild's INSERT is planned against the schema as it
 was before this script ran, so it lists no UserId to copy across — the backfill would be silently
 discarded and the NOT NULL column left empty. Emptying the table instead makes the rebuild a no-op,
 which also clears the data-loss check that dropping Name would otherwise trip.

 The account links go with them: they carry a foreign key to the member rows, so they cannot outlive
 them and are restored alongside.

 A member whose accounts are owned by nobody, or by more than one person, cannot be resolved. It is
 staged with a null UserId, and the post-deployment restore fails loudly on the NOT NULL rather than
 inventing an identity or quietly dropping the member.
*/
IF OBJECT_ID('dbo.RetirementPlanMember', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.RetirementPlanMember', 'UserId') IS NULL
   AND OBJECT_ID('dbo.__RetirementPlanMemberMigration', 'U') IS NULL
BEGIN
    EXEC ('
        SELECT
            m.Id,
            m.RetirementPlanId,
            owners.UserId,
            m.CurrentAge,
            m.CurrentIncome,
            m.SalarySacrifice,
            m.RetirementAge,
            m.GrowthStrategyId,
            m.AnnualFees,
            m.InsurancePremium
        INTO dbo.__RetirementPlanMemberMigration
        FROM dbo.RetirementPlanMember m
        OUTER APPLY (
            SELECT MIN(o.UserId) AS UserId
            FROM dbo.RetirementPlanMemberAccount a
            JOIN dbo.InstrumentOwner o ON o.InstrumentId = a.InstrumentId
            WHERE a.RetirementPlanMemberId = m.Id
            HAVING COUNT(DISTINCT o.UserId) = 1
        ) owners;

        SELECT Id, RetirementPlanMemberId, InstrumentId
        INTO dbo.__RetirementPlanMemberAccountMigration
        FROM dbo.RetirementPlanMemberAccount;

        DELETE FROM dbo.RetirementPlanMemberAccount;
        DELETE FROM dbo.RetirementPlanMember;
    ');
END
GO
