CREATE TABLE [dbo].[RetirementPlanMemberAccount]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RetirementPlanMemberAccount_Id DEFAULT NEWSEQUENTIALID(),
    [RetirementPlanMemberId] UNIQUEIDENTIFIER NOT NULL,
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_RetirementPlanMemberAccount] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RetirementPlanMemberAccount_RetirementPlanMember] FOREIGN KEY ([RetirementPlanMemberId]) REFERENCES [RetirementPlanMember]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RetirementPlanMemberAccount_Instrument] FOREIGN KEY ([InstrumentId]) REFERENCES [Instrument]([Id]),
    CONSTRAINT [UQ_RetirementPlanMemberAccount] UNIQUE ([RetirementPlanMemberId], [InstrumentId])
)
GO
