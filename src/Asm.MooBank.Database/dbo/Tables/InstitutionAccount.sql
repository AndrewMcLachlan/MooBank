CREATE TABLE dbo.[InstitutionAccount]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [AccountTypeId] INT NOT NULL,
    [AccountControllerId] INT NOT NULL,
    [IncludeInPosition] BIT NOT NULL CONSTRAINT DF_InstitutionAccount_IncludeInPosition DEFAULT 0,
    [InstitutionId] INT NOT NULL,
    [IncludeInBudget] BIT NOT NULL CONSTRAINT DF_InstitutionAccount_IncludeInBudget DEFAULT 0,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstitutionAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_InstitutionAccount_TransactionAccount FOREIGN KEY ([InstrumentId]) REFERENCES TransactionAccount([InstrumentId]),
    CONSTRAINT FK_InstitutionAccount_AccountType FOREIGN KEY (AccountTypeId) REFERENCES AccountType(AccountTypeId),
    CONSTRAINT FK_InstitutionAccount_AccountController FOREIGN KEY (AccountControllerId) REFERENCES AccountController(AccountControllerId),
    CONSTRAINT FK_InstitutionAccount_Institution FOREIGN KEY (InstitutionId) REFERENCES Institution(Id)
)
