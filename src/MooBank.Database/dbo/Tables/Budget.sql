CREATE TABLE [dbo].[Budget]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Budget_Id DEFAULT NEWID(),
    [FamilyId] UNIQUEIDENTIFIER NULL,
    [Year] SMALLINT NOT NULL,
    CONSTRAINT [PK_Budget] PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT [FK_Budget_Family] FOREIGN KEY (FamilyId) REFERENCES [Family](Id),
)

GO

CREATE UNIQUE INDEX [IX_Budget_AccountId_Year] ON [dbo].[Budget] ([FamilyId], [Year])
