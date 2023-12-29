CREATE TABLE [dbo].[ExchangeRate]
(
	[Id] INT IDENTITY(1,1),
    [From] CHAR(3) NOT NULL,
    [To] CHAR(3) NOT NULL,
    [Rate] DECIMAL(10, 4) NOT NULL,
    [ReverseRate] AS 1.0/[Rate],
    [LastUpdated] DATETIME2 NOT NULL CONSTRAINT DF_ExchangeRate_DateTime DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ExchangeRate PRIMARY KEY (Id),
)
GO

CREATE UNIQUE INDEX IX_ExchangeRate_FromTo ON [dbo].[ExchangeRate] ([From], [To])
GO
