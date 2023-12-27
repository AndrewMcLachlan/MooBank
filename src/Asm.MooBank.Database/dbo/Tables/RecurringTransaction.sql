﻿CREATE TABLE [dbo].[RecurringTransaction]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RecurringTransaction_Id DEFAULT NEWID(),
    [VirtualAccountId] UNIQUEIDENTIFIER NOT NULL,
    [Description] VARCHAR(50) NULL,
    [ScheduleId] INT NOT NULL,
    [Amount] DECIMAL(10, 2) NOT NULL,
    [LastRun] DATETIME2 NULL,
    CONSTRAINT [PK_RecuringTransaction] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_RecurringTransaction_Schedule] FOREIGN KEY ([ScheduleId]) REFERENCES [Schedule]([ScheduleId]),
    CONSTRAINT [FK_RecurringTransaction_VirtualAccount] FOREIGN KEY ([VirtualAccountId]) REFERENCES [dbo].[VirtualAccount]([AccountId])
)
