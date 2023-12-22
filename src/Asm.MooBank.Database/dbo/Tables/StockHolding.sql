CREATE TABLE [dbo].[StockHolding]
(
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Symbol CHAR(3) NOT NULL,
    Exchange CHAR(2) NOT NULL CONSTRAINT DF_StockHolding_Exchange DEFAULT 'AU',
    Quantity AS dbo.StockQuantity(AccountId),
    CurrentPrice DECIMAL(18, 2) NOT NULL,
    [CurrentValue] AS dbo.StockValue(AccountId, CurrentPrice),
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_StockHolding_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_StockHolding PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_StockHolding_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
)