CREATE TABLE [utilities].[Account]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AccountId DEFAULT (NEWID()),
    [AccountNumber] VARCHAR(15) NULL,
    [InstitutionId] INT NULL,
    [UtilityTypeId] INT NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ([InstrumentId] ASC),
    CONSTRAINT [FK_Account_UtilityType] FOREIGN KEY([UtilityTypeId]) REFERENCES [utilities].[UtilityType] ([Id]),
    CONSTRAINT [FK_Account_Instrument] FOREIGN KEY([InstrumentId]) REFERENCES [dbo].[Instrument]([Id]),
    CONSTRAINT [FK_Account_Institution] FOREIGN KEY([InstitutionId]) REFERENCES [dbo].[Institution]([Id]),
)