CREATE TABLE [dbo].[VirtualInstrument]
(
    [InstrumentId] UNIQUEIDENTIFIER,
    [ParentInstrumentId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_VirtualInstrument PRIMARY KEY CLUSTERED ([InstrumentId]),
    CONSTRAINT FK_VirtualInstrument_Instrument FOREIGN KEY ([InstrumentId]) REFERENCES [dbo].[Instrument]([Id]),
    CONSTRAINT FK_VirtualInstrument_Instrument_Parent FOREIGN KEY ([ParentInstrumentId]) REFERENCES [dbo].[Instrument]([Id])
)
GO

