CREATE TABLE [macquarie].[TransactionRaw]
(
    [Id] UNIQUEIDENTIFIER CONSTRAINT DF_Macquarie_TransactionRaw_Id DEFAULT NEWID(),
    [TransactionId] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Date] DATE NOT NULL DEFAULT SYSDATETIME(),
  [Details] NVARCHAR(500) NULL,
    [Account] NVARCHAR(100) NULL,
    [Category] NVARCHAR(100) NULL,
    [Subcategory] NVARCHAR(100) NULL,
    [Tags] NVARCHAR(500) NULL,
    [Notes] NVARCHAR(1000) NULL,
    [Debit] DECIMAL(12, 4) NULL,
    [Credit] DECIMAL(12, 4) NULL,
    [Balance] DECIMAL(12, 4) NULL,
    [OriginalDescription] NVARCHAR(500) NULL,
    [SequenceNumber] INT NULL,
    [Imported] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_Macquarie_TransactionRaw] PRIMARY KEY CLUSTERED (Id),
)

GO
