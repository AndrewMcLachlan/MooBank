CREATE TABLE [dbo].[StockHolding]
(
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Symbol CHAR(5) NOT NULL,
    Exchange CHAR(2) NULL,
    Quantity AS dbo.StockQuantity(AccountId),
    CurrentPrice DECIMAL(12, 4) NOT NULL,
    [CurrentValue] AS dbo.StockValue(AccountId, CurrentPrice),
    GainLoss AS dbo.StockGainLoss(AccountId, CurrentPrice),
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_StockHolding_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_StockHolding PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_StockHolding_Account FOREIGN KEY (AccountId) REFERENCES [Instrument]([Id]),
)