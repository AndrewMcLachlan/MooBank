CREATE TABLE [dbo].[Tag]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(50) NOT NULL,
    [FamilyId] UNIQUEIDENTIFIER NOT NULL,
    [Colour] CHAR(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Tag_Deleted] DEFAULT 0,
    CONSTRAINT [PK_TransactionTag] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Tag_Family] FOREIGN KEY (FamilyId) REFERENCES [Family](Id)
)

GO

CREATE UNIQUE INDEX [IX_Tag_Name] ON [dbo].[Tag] ([Name], [FamilyId]) WHERE Deleted = 0
