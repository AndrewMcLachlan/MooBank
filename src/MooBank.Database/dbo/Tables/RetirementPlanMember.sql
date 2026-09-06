CREATE TABLE [dbo].[RetirementPlanMember]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RetirementPlanMember_Id DEFAULT NEWID(),
    [RetirementPlanId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [CurrentAge] INT NOT NULL,
    [CurrentIncome] DECIMAL(18,2) NOT NULL,
    [SalarySacrifice] DECIMAL(18,2) NOT NULL CONSTRAINT DF_RetirementPlanMember_SalarySacrifice DEFAULT 0,
    [RetirementAge] INT NOT NULL,
    [GrowthStrategyId] TINYINT NOT NULL CONSTRAINT DF_RetirementPlanMember_GrowthStrategyId DEFAULT 0,
    -- Only read when the strategy is Custom (0); a named strategy takes its rate from
    -- GrowthStrategyRate so an admin correction reaches every plan.
    [CustomReturnRate] DECIMAL(6,4) NULL,
    -- The strategy this member moves to once retired, and its rate when that strategy is Custom.
    -- Absent means they stay where they are.
    [RetirementGrowthStrategyId] TINYINT NULL,
    [RetirementCustomReturnRate] DECIMAL(6,4) NULL,
    -- How this member's salary grows. Absent follows the plan's inflation, which is the assumption
    -- that pay keeps pace with prices; a figure here says otherwise.
    [SalaryGrowthRate] DECIMAL(6,4) NULL,
    [AnnualFees] DECIMAL(18,2) NOT NULL CONSTRAINT DF_RetirementPlanMember_AnnualFees DEFAULT 0,
    [InsurancePremium] DECIMAL(18,2) NOT NULL CONSTRAINT DF_RetirementPlanMember_InsurancePremium DEFAULT 0,
    CONSTRAINT [PK_RetirementPlanMember] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RetirementPlanMember_RetirementPlan] FOREIGN KEY ([RetirementPlanId]) REFERENCES [RetirementPlan]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RetirementPlanMember_User] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]),
    CONSTRAINT [FK_RetirementPlanMember_GrowthStrategy] FOREIGN KEY ([GrowthStrategyId]) REFERENCES [GrowthStrategy]([Id]),
    -- One member per person: two rows for the same user would count their balance twice.
    CONSTRAINT [UQ_RetirementPlanMember_Person] UNIQUE ([RetirementPlanId], [UserId]),
    CONSTRAINT [CK_RetirementPlanMember_CurrentAge] CHECK ([CurrentAge] BETWEEN 0 AND 120),
    CONSTRAINT [CK_RetirementPlanMember_RetirementAge] CHECK ([RetirementAge] BETWEEN 1 AND 120),
    CONSTRAINT [CK_RetirementPlanMember_CurrentIncome] CHECK ([CurrentIncome] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_SalarySacrifice] CHECK ([SalarySacrifice] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_AnnualFees] CHECK ([AnnualFees] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_InsurancePremium] CHECK ([InsurancePremium] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_CustomReturnRate] CHECK ([CustomReturnRate] BETWEEN -1 AND 1),
    -- A custom strategy with no rate has nothing to grow by, and a named strategy with one
    -- would carry a figure that is never read.
    CONSTRAINT [CK_RetirementPlanMember_CustomRateMatchesStrategy] CHECK (
        ([GrowthStrategyId] = 0 AND [CustomReturnRate] IS NOT NULL) OR
        ([GrowthStrategyId] <> 0 AND [CustomReturnRate] IS NULL)),
    CONSTRAINT [CK_RetirementPlanMember_SalaryGrowthRate] CHECK ([SalaryGrowthRate] BETWEEN -1 AND 1),
    CONSTRAINT [CK_RetirementPlanMember_RetirementCustomReturnRate] CHECK ([RetirementCustomReturnRate] BETWEEN -1 AND 1),
    -- Same pairing as the accumulation strategy: Custom needs a rate, a named one must not carry
    -- one, and no retirement strategy means no rate either.
    CONSTRAINT [CK_RetirementPlanMember_RetirementRateMatchesStrategy] CHECK (
        ([RetirementGrowthStrategyId] IS NULL AND [RetirementCustomReturnRate] IS NULL) OR
        ([RetirementGrowthStrategyId] = 0 AND [RetirementCustomReturnRate] IS NOT NULL) OR
        ([RetirementGrowthStrategyId] > 0 AND [RetirementCustomReturnRate] IS NULL))
)
GO

CREATE INDEX [IX_RetirementPlanMember_RetirementPlanId] ON [dbo].[RetirementPlanMember] ([RetirementPlanId])
GO
