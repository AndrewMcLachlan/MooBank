CREATE TABLE [dbo].[StockPriceHistory]
(
	[Id] INT IDENTITY(1,1),
    [Symbol] CHAR(3) NOT NULL,
    [Exchange] CHAR(2) NOT NULL CONSTRAINT DF_StockPriceHistory_Exchange DEFAULT 'AU',
    [Price] DECIMAL(12, 4) NOT NULL,
    [Date] DATE NOT NULL,
    CONSTRAINT PK_StockPriceHistory PRIMARY KEY CLUSTERED ([Id]),
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_StockPriceHistory] ON [dbo].[StockPriceHistory] ([Symbol], [Exchange], [Date])
GO
