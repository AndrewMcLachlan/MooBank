CREATE TABLE [aussuper].[TransactionRaw]
(
    [Id] UNIQUEIDENTIFIER CONSTRAINT DF_TransactionRaw_Id DEFAULT NEWID(),
    [TransactionId] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Date] DATE NOT NULL DEFAULT SYSDATETIME(),
    [Category] NVARCHAR(255) NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Description] VARCHAR(255) NULL,
    [PaymentPeriodStart] DATE NULL,
    [PaymentPeriodEnd] DATE NULL,
    [SGContributions] DECIMAL(10, 2) NULL,
    [EmployerAdditional] DECIMAL(10, 2) NULL,
    [SalarySacrifice] DECIMAL(10, 2) NULL,
    [MemberAdditional] DECIMAL(10, 2) NULL,
    [TotalAmount] DECIMAL(10, 2) NOT NULL,
    [Imported] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TransactionRaw] PRIMARY KEY CLUSTERED (Id),
)

GO
