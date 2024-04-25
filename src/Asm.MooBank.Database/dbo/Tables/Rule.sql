CREATE TABLE [dbo].[Rule]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Contains] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(512) NULL,
    CONSTRAINT [PK_Rule] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Rule_Account] FOREIGN KEY (AccountId) REFERENCEs [Instrument]([Id]),
)
