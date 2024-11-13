CREATE TABLE [utilities].[DiscountBill](
    [BillId] [int] NOT NULL,
    [DiscountId] [int] NOT NULL,
    CONSTRAINT [PK_DiscountBill] PRIMARY KEY CLUSTERED ([BillId] ASC, [DiscountId] ASC),
    CONSTRAINT [FK_DiscountBill_Bill] FOREIGN KEY([BillId]) REFERENCES [utilities].[Bill] ([Id]),
    CONSTRAINT [FK_DiscountBill_Discount] FOREIGN KEY([DiscountId]) REFERENCES [utilities].[Discount] ([Id]),
)