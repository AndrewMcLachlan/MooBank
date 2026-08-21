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

GO

/*
 Income used to be modelled twice: a fixed monthly figure on the plan, and planned income items,
 with nothing reconciling them. The fixed figure is also the same in every month by construction,
 so extra income that ends could not be expressed at all — and since spending is modelled as a
 function of income, the expense line was flat forever whatever the fitted slope said.

 The figure is converted into an ordinary planned income item on a monthly schedule, which the
 author can date, end or split as their circumstances change. Manual adjustments were cumulative
 deltas applied from a date onwards, so they become a sequence of items: each runs from its own
 date until the next adjustment, carrying the running total.

 Every plan with a figure is converted, including one that already has income items of its own: a
 household can have two earners, and the fixed figure and the planned items were as likely to be
 complementary as duplicated. Skipping those plans lost the figure silently, and the plan carrying
 both models is exactly the one the double-counting affected.

 The JSON is staged here; the column itself is dropped by the schema comparison, in the main body
 of the deployment that follows. Dropping it here instead does not work: the comparison plans its
 statements against the database as it stands *before* this script runs, so it has already decided
 to drop the column, and finds it gone when its turn comes.

 Two guards, because two things can leave work half done. COL_LENGTH covers the ordinary case: once
 a deployment has succeeded the column is gone and the whole block is skipped. The staging table
 covers a deployment that failed after this script ran but before the column was dropped -- the
 items exist, the column does not, and creating them a second time would double the plan's income.
*/
IF OBJECT_ID('dbo.ForecastPlan', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.ForecastPlan', 'IncomeStrategy') IS NOT NULL
   AND OBJECT_ID('dbo.__ForecastIncomeStrategyMigration', 'U') IS NULL
BEGIN
    -- Wrapped in EXEC so the column reference is bound at runtime; otherwise the batch fails to
    -- parse on any deploy where IncomeStrategy no longer exists.
    EXEC ('
        SELECT p.Id AS PlanId, p.IncomeStrategy, CONVERT(int, 0) AS ItemsCreated
        INTO dbo.__ForecastIncomeStrategyMigration
        FROM dbo.ForecastPlan p
        WHERE p.IncomeStrategy IS NOT NULL AND ISJSON(p.IncomeStrategy) = 1;
    ');

    ;WITH Src AS (
        SELECT m.PlanId,
               TRY_CONVERT(decimal(18,2), JSON_VALUE(m.IncomeStrategy, '$.manualRecurring.amount')) AS BaseAmount,
               COALESCE(TRY_CONVERT(date, JSON_VALUE(m.IncomeStrategy, '$.manualRecurring.startDate')), p.StartDate) AS IncomeStart,
               TRY_CONVERT(date, JSON_VALUE(m.IncomeStrategy, '$.manualRecurring.endDate')) AS IncomeEnd,
               m.IncomeStrategy
        FROM dbo.__ForecastIncomeStrategyMigration m
        JOIN dbo.ForecastPlan p ON p.Id = m.PlanId
    ),
    Adj AS (
        SELECT s.PlanId, a.AdjDate, a.DeltaAmount,
               ROW_NUMBER() OVER (PARTITION BY s.PlanId ORDER BY a.AdjDate) AS Seq
        FROM Src s
        CROSS APPLY OPENJSON(s.IncomeStrategy, '$.manualAdjustments')
            WITH (AdjDate date '$.date', DeltaAmount decimal(18,2) '$.deltaAmount') a
        WHERE a.AdjDate IS NOT NULL
    ),
    Segments AS (
        -- The original amount, running until the first adjustment moved it.
        SELECT s.PlanId, 0 AS Seq, s.BaseAmount AS Amount, s.IncomeStart AS SegStart,
               COALESCE(DATEADD(day, -1, (SELECT MIN(a.AdjDate) FROM Adj a WHERE a.PlanId = s.PlanId)), s.IncomeEnd) AS SegEnd
        FROM Src s
        UNION ALL
        -- One segment per adjustment, carrying the running total forward.
        SELECT a.PlanId, a.Seq,
               s.BaseAmount + (SELECT SUM(a2.DeltaAmount) FROM Adj a2 WHERE a2.PlanId = a.PlanId AND a2.Seq <= a.Seq),
               a.AdjDate,
               COALESCE(DATEADD(day, -1, (SELECT MIN(a3.AdjDate) FROM Adj a3 WHERE a3.PlanId = a.PlanId AND a3.Seq > a.Seq)), s.IncomeEnd)
        FROM Adj a
        JOIN Src s ON s.PlanId = a.PlanId
    )
    SELECT NEWID() AS ItemId, PlanId, Seq, Amount, SegStart, SegEnd
    INTO #ForecastIncomeSegments
    FROM Segments
    WHERE Amount IS NOT NULL AND Amount > 0;

    INSERT INTO dbo.ForecastPlannedItem (Id, ForecastPlanId, ItemType, [Name], Amount, IsIncluded, DateMode)
    SELECT ItemId, PlanId, 1, -- Income
           CASE WHEN Seq = 0 THEN N'Income'
                ELSE CONCAT(N'Income (from ', FORMAT(SegStart, 'MMM yyyy', 'en-AU'), N')') END,
           Amount, 1, 1 -- Schedule
    FROM #ForecastIncomeSegments;

    INSERT INTO dbo.PlannedItemSchedule (PlannedItemId, Frequency, AnchorDate, [Interval], EndDate)
    SELECT ItemId, 3 /* Monthly */, SegStart, 1, SegEnd
    FROM #ForecastIncomeSegments;

    -- Recorded per plan so the post-deployment check can tell a plan this migration converted from
    -- one that merely happens to have an income item already.
    UPDATE m
    SET ItemsCreated = x.Created
    FROM dbo.__ForecastIncomeStrategyMigration m
    JOIN (SELECT PlanId, COUNT(*) AS Created FROM #ForecastIncomeSegments GROUP BY PlanId) x
        ON x.PlanId = m.PlanId;

    DROP TABLE #ForecastIncomeSegments;
END

/*
 Gives every forecast plan a currency, so the column can be made NOT NULL.

 Plans have always been created with one -- CreatePlan falls back to the author's own currency --
 but rows predating that are still null, and a plan with no currency renders every amount in the
 forecast with no symbol. The family's currency is the right answer for a family's plan; AUD is the
 same default the User table already carries, for the case where a family somehow has no members.

 Runs before the column is altered: the deployment plan is worked out first, but pre-deployment
 executes ahead of the schema changes it was planned alongside.

 Idempotent by construction -- once nothing is null there is nothing to update.
*/
IF OBJECT_ID('dbo.ForecastPlan', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.ForecastPlan', 'CurrencyCode') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.ForecastPlan WHERE CurrencyCode IS NULL)
BEGIN
    UPDATE p
    SET p.CurrencyCode = COALESCE(
        (SELECT TOP 1 u.Currency FROM dbo.[User] u WHERE u.FamilyId = p.FamilyId ORDER BY u.Id),
        'AUD')
    FROM dbo.ForecastPlan p
    WHERE p.CurrencyCode IS NULL;

    PRINT 'Backfilled CurrencyCode on forecast plans that had none.';
END

/*
 Creates and seeds utilities.ChargeType ahead of the schema changes that add
 ServiceCharge.ChargeTypeId. That column is NOT NULL defaulting to 1, so the schema phase stamps
 every existing row with 1 and then validates the new foreign key -- which fails unless row 1 is
 already there. Post-deployment runs after the constraint is checked, so it cannot do this.

 Only the columns and key: the schema phase adds the foreign key to UtilityType. Guarded on
 ServiceCharge existing, since a database being created from scratch has no rows to stamp, and the
 utilities schema does not exist yet at this point.
*/
IF OBJECT_ID('utilities.ServiceCharge', 'U') IS NOT NULL
   AND OBJECT_ID('utilities.ChargeType', 'U') IS NULL
BEGIN
    CREATE TABLE [utilities].[ChargeType](
        [Id] INT NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [UtilityTypeId] INT NULL,
        CONSTRAINT [PK_ChargeType] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    INSERT INTO [utilities].[ChargeType] ([Id], [Name], [UtilityTypeId])
    VALUES (1, 'Supply', NULL), (2, 'Water Service', 3), (3, 'Sewerage Service', 3);

    PRINT 'Created and seeded utilities.ChargeType ahead of ServiceCharge.ChargeTypeId.';
END
