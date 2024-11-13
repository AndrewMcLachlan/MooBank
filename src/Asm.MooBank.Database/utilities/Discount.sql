CREATE TABLE [utilities].[Discount]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [DiscountPercent] tinyint NULL,
    [DiscountAmount] decimal(12,4) NULL,
    [Reason] varchar(255) NULL,
    CONSTRAINT [PK_Discount] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Discount] CHECK ((([DiscountPercent] IS NULL OR [DiscountAmount] IS NULL) AND NOT ([DiscountPercent] IS NULL AND [DiscountAmount] IS NULL)))
)