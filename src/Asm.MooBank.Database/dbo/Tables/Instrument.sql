CREATE TABLE [dbo].[Instrument]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_InstrumentId DEFAULT (NEWID()),
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(255) NULL,
    [Currency] CHAR(3) NOT NULL CONSTRAINT DF_Instrument_Currency DEFAULT 'AUD',
    [ControllerId] INT NULL,
    [ShareWithFamily] BIT NOT NULL CONSTRAINT DF_Instrument_ShareWithFamily DEFAULT 0,
    [Slug] VARCHAR(50) NULL,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_LastUpdated DEFAULT SYSUTCDATETIME(),
    [ClosedDate] DATE NULL,
    CONSTRAINT PK_Instrument PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT FK_Instrument_Controller FOREIGN KEY (ControllerId) REFERENCES [Controller]([Id]),
)

GO
