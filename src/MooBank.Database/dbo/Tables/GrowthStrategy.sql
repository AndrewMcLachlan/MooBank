-- The investment options a member's superannuation can be projected under, and the assumed
-- long-run nominal return of each.
--
-- The rate is reference data rather than a plan setting: it is an assumption about a market, not
-- about a household, so one set applies to every plan and an admin correction reaches them all at
-- once. Nothing is stored against a projection -- every one is recalculated on the values in force
-- when it runs -- so there is no dated series here as there is for PensionRate.
--
-- The seeded rates are ASIC's MoneySmart figures, which are net of investment fees and of the tax
-- on fund earnings. Administration fees and insurance premiums are charged separately, against the
-- member.
--
-- Custom carries no rate: it means "the figure this member chose", which is stored on the member.
CREATE TABLE [dbo].[GrowthStrategy]
(
    [Id] TINYINT NOT NULL,
    [Description] NVARCHAR(50) NOT NULL,
    [Rate] DECIMAL(6,4) NULL,
    CONSTRAINT [PK_GrowthStrategy] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [CK_GrowthStrategy_Rate] CHECK ([Rate] BETWEEN -1 AND 1),
    -- Id 0 is Custom. Every other strategy must be able to answer what it grows by.
    CONSTRAINT [CK_GrowthStrategy_RateRequired] CHECK (
        ([Id] = 0 AND [Rate] IS NULL) OR ([Id] <> 0 AND [Rate] IS NOT NULL))
)
GO
