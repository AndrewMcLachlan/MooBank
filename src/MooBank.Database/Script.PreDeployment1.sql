/*
 Pre-Deployment Script
 Runs before the schema comparison's statements, but note that the comparison plans those statements
 against the database as it stands *before* this script runs. So this can prepare data the plan is
 about to move or constrain -- staging rows out of a table it will rebuild, backfilling a column it
 is about to make NOT NULL -- but it cannot create or drop an object the plan has already decided to
 create or drop.

 One-off migrations live here only until every environment has run them; they are removed once
 spent.
*/

/*
 A custom return rate moves from the plan onto the member who chose it. The plan column is dropped
 by this deployment and the member column does not exist yet, so the values are staged here while
 the old column still holds them and are read back once the new one arrives.
*/
IF COL_LENGTH('dbo.RetirementPlan', 'ExpectedReturnRate') IS NOT NULL
    AND OBJECT_ID('dbo.CustomReturnRateStaging') IS NULL
BEGIN
    SELECT m.[Id] AS [MemberId], p.[ExpectedReturnRate] AS [Rate]
    INTO [dbo].[CustomReturnRateStaging]
    FROM [dbo].[RetirementPlanMember] m
    INNER JOIN [dbo].[RetirementPlan] p ON p.[Id] = m.[RetirementPlanId]
    WHERE m.[GrowthStrategyId] = 0;
END

GO
