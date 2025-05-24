CREATE TABLE [dbo].[CPIChange]
(
    [Id] INT IDENTITY(1,1),
    [Year] INT NOT NULL,
    [Quarter] INT NOT NULL,
    [ChangePercent] DECIMAL(7,4),
    CONSTRAINT PK_CPIChange PRIMARY KEY CLUSTERED ([Id])
)
