CREATE TABLE dbo.[InstitutionAccount]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [InstitutionId] INT NOT NULL,
    [ImporterTypeId] INT NULL,
    [OpenedDate] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstitutionAccount_OpenedDate DEFAULT SYSUTCDATETIME(),
    [ClosedDate] DATETIMEOFFSET(0) NULL,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstitutionAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT FK_InstitutionAccount_LogicalAccount FOREIGN KEY ([InstrumentId]) REFERENCES [LogicalAccount]([InstrumentId]),
    CONSTRAINT FK_InstitutionAccount_Institution FOREIGN KEY (InstitutionId) REFERENCES Institution(Id)
)
