/*
 What a service charge is for. A water bill carries two: water supply and sewerage.
*/
CREATE TABLE [utilities].[ChargeType](
    [Id] INT NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    -- NULL applies to any utility; set, it keeps sewerage off an electricity bill.
    [UtilityTypeId] INT NULL,
    CONSTRAINT [PK_ChargeType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ChargeType_UtilityType] FOREIGN KEY ([UtilityTypeId]) REFERENCES [utilities].[UtilityType] ([Id])
)
