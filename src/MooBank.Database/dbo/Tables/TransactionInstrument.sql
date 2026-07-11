CREATE TABLE [dbo].[TransactionInstrument]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_TransactionAccount PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_TransactionAccount_Account FOREIGN KEY ([InstrumentId]) REFERENCES [Instrument]([Id]),
)

GO
