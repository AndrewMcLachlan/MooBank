CREATE TABLE [dbo].[RetirementPlanMember]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RetirementPlanMember_Id DEFAULT NEWID(),
    [RetirementPlanId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [DateOfBirth] DATE NOT NULL,
    [CurrentIncome] DECIMAL(18,2) NOT NULL,
    [RetirementAge] INT NOT NULL,
    CONSTRAINT [PK_RetirementPlanMember] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RetirementPlanMember_RetirementPlan] FOREIGN KEY ([RetirementPlanId]) REFERENCES [RetirementPlan]([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_RetirementPlanMember_RetirementAge] CHECK ([RetirementAge] BETWEEN 1 AND 120),
    CONSTRAINT [CK_RetirementPlanMember_CurrentIncome] CHECK ([CurrentIncome] >= 0)
)
GO

CREATE INDEX [IX_RetirementPlanMember_RetirementPlanId] ON [dbo].[RetirementPlanMember] ([RetirementPlanId])
GO
