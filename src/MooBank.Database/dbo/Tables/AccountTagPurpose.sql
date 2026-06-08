CREATE TABLE [dbo].[AccountTagPurpose]
(
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    [Purpose] TINYINT NOT NULL,
    [TagId] INT NOT NULL,
    CONSTRAINT [PK_AccountTagPurpose] PRIMARY KEY CLUSTERED ([InstrumentId], [Purpose]),
    CONSTRAINT [FK_AccountTagPurpose_LogicalAccount] FOREIGN KEY ([InstrumentId]) REFERENCES [LogicalAccount]([InstrumentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AccountTagPurpose_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id]),
)
