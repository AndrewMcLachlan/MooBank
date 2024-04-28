CREATE TABLE [dbo].[Asset]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [PurchasePrice] DECIMAL(12,4) NOT NULL,
    CONSTRAINT PK_Asset PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_Asset_Account FOREIGN KEY ([InstrumentId]) REFERENCES [Instrument]([Id]),
)
