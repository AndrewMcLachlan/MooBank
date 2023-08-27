CREATE TABLE [dbo].[Tag]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(50) NOT NULL,
    [FamilyId] UNIQUEIDENTIFIER NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_TransactionTag_Deleted] DEFAULT 0,
    CONSTRAINT [PK_TransactionTag] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Tag_Family] FOREIGN KEY (FamilyId) REFERENCES [Family](Id)
)

GO

CREATE UNIQUE INDEX [IX_TransactionTag_Name] ON [dbo].[Tag] ([Name]) WHERE Deleted = 0
