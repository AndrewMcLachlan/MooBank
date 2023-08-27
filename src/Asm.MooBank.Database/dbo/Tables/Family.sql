CREATE TABLE [dbo].[Family] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Family_Id DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_Family PRIMARY KEY (Id),
)