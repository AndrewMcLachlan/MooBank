CREATE TABLE [ing].[TransactionExtra]
(
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    [Description] NVARCHAR(150) NULL,
    [PurchaseType] NVARCHAR(20) NULL,
    [ReceiptNumber] INT NULL,
    [Location] NVARCHAR(12) NULL,
    [PurchaseDate] DATE NULL,
    [Reference] NVARCHAR(150) NULL,
    [Last4Digits] SMALLINT NULL,
    CONSTRAINT PK_TransactionExtra PRIMARY KEY CLUSTERED (TransactionId),
    CONSTRAINT FK_TransactionExtra_Transaction FOREIGN KEY (TransactionId) REFERENCES dbo.[Transaction](TransactionId),
)
