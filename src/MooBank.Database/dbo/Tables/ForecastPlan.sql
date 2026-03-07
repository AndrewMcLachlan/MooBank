CREATE TABLE [dbo].[ForecastPlan]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ForecastPlan_Id DEFAULT NEWID(),
    [FamilyId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,
    [AccountScopeMode] TINYINT NOT NULL CONSTRAINT DF_ForecastPlan_AccountScopeMode DEFAULT 0,
    [StartingBalanceMode] TINYINT NOT NULL CONSTRAINT DF_ForecastPlan_StartingBalanceMode DEFAULT 0,
    [StartingBalanceAmount] DECIMAL(18,2) NULL,
    [CurrencyCode] CHAR(3) NULL,
    [IncomeStrategy] NVARCHAR(MAX) NULL,
    [OutgoingStrategy] NVARCHAR(MAX) NULL,
    [Assumptions] NVARCHAR(MAX) NULL,
    [IsArchived] BIT NOT NULL CONSTRAINT DF_ForecastPlan_IsArchived DEFAULT 0,
    [CreatedUtc] DATETIME2 NOT NULL CONSTRAINT DF_ForecastPlan_CreatedUtc DEFAULT SYSUTCDATETIME(),
    [UpdatedUtc] DATETIME2 NOT NULL CONSTRAINT DF_ForecastPlan_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_ForecastPlan] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ForecastPlan_Family] FOREIGN KEY ([FamilyId]) REFERENCES [Family]([Id]),
    CONSTRAINT [CK_ForecastPlan_DateRange] CHECK ([EndDate] <= DATEADD(YEAR, 10, [StartDate]))
)
GO

CREATE INDEX [IX_ForecastPlan_FamilyId] ON [dbo].[ForecastPlan] ([FamilyId])
GO
