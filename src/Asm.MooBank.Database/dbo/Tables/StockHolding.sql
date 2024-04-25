CREATE TABLE [dbo].[StockHolding]
(
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    Symbol CHAR(5) NOT NULL,
    Exchange CHAR(2) NULL,
    Quantity AS dbo.StockQuantity(InstrumentId),
    CurrentPrice DECIMAL(12, 4) NOT NULL,
    [CurrentValue] AS dbo.StockValue(InstrumentId, CurrentPrice),
    GainLoss AS dbo.StockGainLoss(InstrumentId, CurrentPrice),
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_StockHolding_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_StockHolding PRIMARY KEY CLUSTERED (InstrumentId),
    CONSTRAINT FK_StockHolding_Account FOREIGN KEY (InstrumentId) REFERENCES [Instrument]([Id]),
)