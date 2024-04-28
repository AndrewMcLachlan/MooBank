CREATE TABLE [dbo].[Instrument]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AccountId DEFAULT (NEWID()),
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(255) NULL,
    [Currency] CHAR(3) NOT NULL CONSTRAINT DF_Account_Currency DEFAULT 'AUD',
    [Balance] DECIMAL(12, 4) NOT NULL CONSTRAINT DF_AccountBalance DEFAULT 0,
    [ControllerId] INT NULL,
    [ShareWithFamily] BIT NOT NULL CONSTRAINT DF_Account_ShareWithFamily DEFAULT 0,
    [Slug] VARCHAR(50) NULL,
    [LastUpdated] DATETIMEOFFSET(0) NOT NULL CONSTRAINT DF_LastUpdated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Instrument PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT FK_Instrument_Controller FOREIGN KEY (ControllerId) REFERENCES [Controller]([Id]),
)

GO
