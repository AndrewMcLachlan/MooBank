CREATE TABLE [dbo].[RetirementPlan]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RetirementPlan_Id DEFAULT NEWID(),
    [FamilyId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [ExpectedReturnRate] DECIMAL(6,4) NOT NULL,
    [InflationRate] DECIMAL(6,4) NOT NULL,
    [SuperGuaranteeRate] DECIMAL(6,4) NOT NULL,
    [ContributionsTaxRate] DECIMAL(6,4) NOT NULL,
    [LifeExpectancy] INT NOT NULL,
    [CreatedUtc] DATETIME2 NOT NULL CONSTRAINT DF_RetirementPlan_CreatedUtc DEFAULT SYSUTCDATETIME(),
    [UpdatedUtc] DATETIME2 NOT NULL CONSTRAINT DF_RetirementPlan_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_RetirementPlan] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RetirementPlan_Family] FOREIGN KEY ([FamilyId]) REFERENCES [Family]([Id]),
    CONSTRAINT [CK_RetirementPlan_ExpectedReturnRate] CHECK ([ExpectedReturnRate] BETWEEN -1 AND 1),
    CONSTRAINT [CK_RetirementPlan_InflationRate] CHECK ([InflationRate] BETWEEN -1 AND 1),
    CONSTRAINT [CK_RetirementPlan_SuperGuaranteeRate] CHECK ([SuperGuaranteeRate] BETWEEN 0 AND 1),
    CONSTRAINT [CK_RetirementPlan_ContributionsTaxRate] CHECK ([ContributionsTaxRate] BETWEEN 0 AND 1),
    CONSTRAINT [CK_RetirementPlan_LifeExpectancy] CHECK ([LifeExpectancy] BETWEEN 1 AND 120)
)
GO

CREATE INDEX [IX_RetirementPlan_FamilyId] ON [dbo].[RetirementPlan] ([FamilyId])
GO
