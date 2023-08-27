CREATE TABLE dbo.[InstitutionAccount]
(
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [AccountTypeId] INT NOT NULL,
    [AccountControllerId] INT NOT NULL,
    [IncludeInPosition] BIT NOT NULL CONSTRAINT DF_InstiutionAccount_IncludeInPosition DEFAULT 0,
    [InstitutionId] INT NULL,
    [ShareWithFamily] BIT NOT NULL CONSTRAINT DF_InstiutionAccount_ShareWithFamily DEFAULT 0,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_InstiutionAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_InstitutionAccount PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_InstitutionAccount_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
    CONSTRAINT FK_InstitutionAccount_AccountType FOREIGN KEY (AccountTypeId) REFERENCES AccountType(AccountTypeId),
    CONSTRAINT FK_InstitutionAccount_AccountController FOREIGN KEY (AccountControllerId) REFERENCES AccountController(AccountControllerId)
)
