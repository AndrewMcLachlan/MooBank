﻿CREATE TABLE [dbo].[Transaction]
(
    [TransactionId] UNIQUEIDENTIFIER CONSTRAINT DF_TransactionId DEFAULT NEWID(),
    [TransactionReference] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [AccountHolderId] UNIQUEIDENTIFIER NULL,
    [TransactionTypeId] INT NOT NULL,
    [TransactionSubTypeId] INT NULL,
    [Amount] DECIMAL(12, 4) NOT NULL,
    [Description] VARCHAR(255) NULL,
    [Location] NVARCHAR(150) NULL,
    [Reference] NVARCHAR(150) NULL,
    [PurchaseDate] DATE NULL,
    [TransactionTime] DATETIME2 NOT NULL CONSTRAINT DF_TransactionTime DEFAULT SYSDATETIME(),
    [Notes] NVARCHAR(512) NULL,
    [Extra] NVARCHAR(4000) NULL,
    [ExcludeFromReporting] BIT NOT NULL CONSTRAINT DF_Transaction_ExcludeFromReporting DEFAULT(0),
    [Created] DATETIME2 NOT NULL CONSTRAINT [DF_Transaction_Created] DEFAULT SYSDATETIME(),
    [Source] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Transaction_Source] DEFAULT 'Unknown',
    CONSTRAINT [PK_Transaction] PRIMARY KEY CLUSTERED (TransactionId),
    CONSTRAINT [FK_Transaction_Account] FOREIGN KEY ([AccountId]) REFERENCES [Instrument]([Id]),
    CONSTRAINT [FK_Transaction_AccountHolder] FOREIGN KEY ([AccountHolderId]) REFERENCES [User]([Id]),
    CONSTRAINT [FK_Transaction_TransactionType] FOREIGN KEY ([TransactionTypeId]) REFERENCES [TransactionType]([TransactionTypeId]),
)

GO

CREATE NONCLUSTERED INDEX IX_Transaction_AccountId_TransactionTime
ON dbo.[Transaction] (AccountId, TransactionTime);
GO
