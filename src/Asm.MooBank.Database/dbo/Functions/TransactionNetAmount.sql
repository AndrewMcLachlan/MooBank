﻿CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @OffsetByTransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN ISNULL((SELECT Amount FROM [Transaction] WHERE TransactionId = @OffsetByTransactionId), 0) + @Amount
END