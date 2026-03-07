CREATE TABLE dbo.[InstitutionAccount]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(255) NOT NULL CONSTRAINT DF_InstitutionAccount_Name DEFAULT '',
    [InstitutionId] INT NOT NULL,
    [ImporterTypeId] INT NULL,
    [BSB] CHAR(6) NULL,
    [AccountNumber] VARCHAR(20) NULL,
    [OpenedDate] DATE NOT NULL CONSTRAINT DF_InstitutionAccount_OpenedDate DEFAULT SYSUTCDATETIME(),
    [ClosedDate] DATE NULL,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstitutionAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT FK_InstitutionAccount_LogicalAccount FOREIGN KEY ([InstrumentId]) REFERENCES [LogicalAccount]([InstrumentId]),
    CONSTRAINT FK_InstitutionAccount_Institution FOREIGN KEY (InstitutionId) REFERENCES Institution(Id)
)
