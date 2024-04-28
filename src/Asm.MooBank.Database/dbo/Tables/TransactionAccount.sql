CREATE TABLE [dbo].[TransactionInstrument]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [LastTransaction] AS dbo.LastTransaction([InstrumentId]),
    [CalculatedBalance] AS dbo.AccountBalance([InstrumentId]),
    CONSTRAINT PK_TransactionAccount PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_TransactionAccount_Account FOREIGN KEY ([InstrumentId]) REFERENCES [Instrument]([Id]),
)

GO
