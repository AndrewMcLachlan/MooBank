CREATE TABLE dbo.[LogicalAccount]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [AccountTypeId] INT NOT NULL,
    [IncludeInBudget] BIT NOT NULL CONSTRAINT DF_LogicalAccount_IncludeInBudget DEFAULT 0,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_LogicalAccount_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_LogicalAccount PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_LogicalAccount_TransactionAccount FOREIGN KEY ([InstrumentId]) REFERENCES [TransactionInstrument]([InstrumentId]),
    CONSTRAINT FK_LogicalAccount_AccountType FOREIGN KEY (AccountTypeId) REFERENCES [AccountType]([Id]),
)
