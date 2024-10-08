CREATE TABLE [dbo].[Institution]
(
    [Id] INT IDENTITY(1,1),
    [Name] NVARCHAR(255) NOT NULL,
    [InstitutionTypeId] INT NOT NULL,
    CONSTRAINT PK_Institution PRIMARY KEY (Id),
    CONSTRAINT FK_Institution_InstitutionType FOREIGN KEY (InstitutionTypeId) REFERENCES InstitutionType(Id)
)