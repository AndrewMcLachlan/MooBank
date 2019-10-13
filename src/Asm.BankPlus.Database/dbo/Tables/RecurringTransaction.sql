CREATE TABLE [dbo].[RecurringTransaction]
(
	[RecurringTransactionId] INT NOT NULL PRIMARY KEY IDENTITY,
    [SourceVirtualAccountId] UNIQUEIDENTIFIER NOT NULL,
    [DestinationVirtualAccountId] UNIQUEIDENTIFIER NOT NULL,
    [Description] VARCHAR(50) NULL,
    [ScheduleId] INT NOT NULL,
    [Amount] DECIMAL(10, 2) NOT NULL,
    [LastRun] DATETIME2 NULL,
    CONSTRAINT [FK_RecurringTransaction_Schedule] FOREIGN KEY ([ScheduleId]) REFERENCES [Schedule]([ScheduleId]),
    CONSTRAINT [FK_RecurringTransaction_VirtualAccount_Source] FOREIGN KEY ([SourceVirtualAccountId]) REFERENCES [dbo].[VirtualAccount]([AccountId]),
    CONSTRAINT [FK_RecurringTransaction_VirtualAccount_Destination] FOREIGN KEY ([DestinationVirtualAccountId]) REFERENCES [dbo].[VirtualAccount]([AccountId])
)
