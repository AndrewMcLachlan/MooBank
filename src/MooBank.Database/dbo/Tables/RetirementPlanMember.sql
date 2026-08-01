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
    [AnnualFees] DECIMAL(18,2) NOT NULL CONSTRAINT DF_RetirementPlanMember_AnnualFees DEFAULT 0,
    [InsurancePremium] DECIMAL(18,2) NOT NULL CONSTRAINT DF_RetirementPlanMember_InsurancePremium DEFAULT 0,
    CONSTRAINT [PK_RetirementPlanMember] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RetirementPlanMember_RetirementPlan] FOREIGN KEY ([RetirementPlanId]) REFERENCES [RetirementPlan]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RetirementPlanMember_User] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]),
    -- One member per person: two rows for the same user would count their balance twice.
    CONSTRAINT [UQ_RetirementPlanMember_Person] UNIQUE ([RetirementPlanId], [UserId]),
    CONSTRAINT [CK_RetirementPlanMember_CurrentAge] CHECK ([CurrentAge] BETWEEN 0 AND 120),
    CONSTRAINT [CK_RetirementPlanMember_RetirementAge] CHECK ([RetirementAge] BETWEEN 1 AND 120),
    CONSTRAINT [CK_RetirementPlanMember_CurrentIncome] CHECK ([CurrentIncome] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_SalarySacrifice] CHECK ([SalarySacrifice] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_AnnualFees] CHECK ([AnnualFees] >= 0),
    CONSTRAINT [CK_RetirementPlanMember_InsurancePremium] CHECK ([InsurancePremium] >= 0)
)
GO

CREATE INDEX [IX_RetirementPlanMember_RetirementPlanId] ON [dbo].[RetirementPlanMember] ([RetirementPlanId])
GO
