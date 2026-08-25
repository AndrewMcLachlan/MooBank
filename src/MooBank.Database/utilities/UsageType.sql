/*
 What a metered quantity on a bill measures. Export is electricity sent back to the grid, which the
 retailer credits rather than charges.
*/
CREATE TABLE [utilities].[UsageType](
    [Id] INT NOT NULL,
    [Name] NVARCHAR(20) NOT NULL,
    CONSTRAINT [PK_UsageType] PRIMARY KEY CLUSTERED ([Id] ASC)
)
