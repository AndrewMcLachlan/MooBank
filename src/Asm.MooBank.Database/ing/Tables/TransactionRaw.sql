﻿CREATE TABLE [ing].[TransactionRaw]
(
    [Id] UNIQUEIDENTIFIER CONSTRAINT DF_TransactionRaw_Id DEFAULT NEWID(),
    [TransactionId] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Date] DATE NOT NULL DEFAULT SYSDATETIME(),
    [Description] VARCHAR(255) NULL,
    [Credit] DECIMAL(10, 2) NULL,
    [Debit] DECIMAL(10, 2) NULL,
    [Balance] DECIMAL(10, 2) NULL,
    [Imported] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TransactionRaw] PRIMARY KEY CLUSTERED (Id),
)

GO