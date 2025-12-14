CREATE TABLE [dbo].[TransactionInstrument]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [LastTransaction] DATE NULL,
    [Balance] DECIMAL(12, 4) NOT NULL CONSTRAINT DF_TransactionInstrument_Balance DEFAULT 0,
    CONSTRAINT PK_TransactionAccount PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_TransactionAccount_Account FOREIGN KEY ([InstrumentId]) REFERENCES [Instrument]([Id]),
)

GO
