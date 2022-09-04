CREATE TABLE [ing].[TransactionExtra]
(
	[TransactionId] UNIQUEIDENTIFIER NOT NULL,
    [Description] NVARCHAR(50) NULL,
    [PurchaseType] NVARCHAR(20) NULL,
    [ReceiptNumber] INT NULL,
    [Location] NVARCHAR(12) NULL,
    [PurchaseDate] DATE NULL,
    [Reference] NVARCHAR(50) NULL,
    CONSTRAINT PK_TransactionExtra PRIMARY KEY CLUSTERED (TransactionId),
    CONSTRAINT FK_TransactionExtra_Transaction FOREIGN KEY (TransactionId) REFERENCES dbo.[Transaction](TransactionId),
)
