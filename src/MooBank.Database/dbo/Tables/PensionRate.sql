-- Age Pension rates and thresholds, as a dated series: Services Australia reindexes them in March
-- and September, and a projection should use the set in force on the day it runs.
--
-- There is no official feed for these, so they are entered by hand and need checking against the
-- published rates. The asset free areas are the homeowner ones; non-homeowner figures are higher and
-- are not modelled separately.
CREATE TABLE [dbo].[PensionRate]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [EffectiveFrom] DATE NOT NULL,
    [EligibilityAge] INT NOT NULL,
    [MaxAnnualSingle] DECIMAL(18,2) NOT NULL,
    [MaxAnnualCouple] DECIMAL(18,2) NOT NULL,
    [AssetsFreeAreaSingle] DECIMAL(18,2) NOT NULL,
    [AssetsFreeAreaCouple] DECIMAL(18,2) NOT NULL,
    -- $3 a fortnight per $1,000 over the free area is $78 a year per $1,000: a rate of 0.078.
    [AssetsTaperRate] DECIMAL(6,4) NOT NULL,
    CONSTRAINT [PK_PensionRate] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_PensionRate_EffectiveFrom] UNIQUE ([EffectiveFrom]),
    CONSTRAINT [CK_PensionRate_EligibilityAge] CHECK ([EligibilityAge] BETWEEN 50 AND 80),
    CONSTRAINT [CK_PensionRate_MaxAnnualSingle] CHECK ([MaxAnnualSingle] >= 0),
    CONSTRAINT [CK_PensionRate_MaxAnnualCouple] CHECK ([MaxAnnualCouple] >= 0),
    CONSTRAINT [CK_PensionRate_AssetsFreeAreaSingle] CHECK ([AssetsFreeAreaSingle] >= 0),
    CONSTRAINT [CK_PensionRate_AssetsFreeAreaCouple] CHECK ([AssetsFreeAreaCouple] >= 0),
    CONSTRAINT [CK_PensionRate_AssetsTaperRate] CHECK ([AssetsTaperRate] BETWEEN 0 AND 1)
)
