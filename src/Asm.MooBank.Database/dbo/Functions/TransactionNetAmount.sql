CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @TransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(12,4)
)
RETURNS DECIMAL(12,4)
AS
BEGIN
    DECLARE @OffsetAmount DECIMAL(12,4)
    DECLARE @NetAmount DECIMAL(12,4)

    SET @OffsetAmount = ISNULL((SELECT SUM(tso.Amount) FROM [TransactionSplitOffset] tso INNER JOIN [TransactionSplit] ts ON ts.Id = tso.TransactionSplitId WHERE ts.TransactionId = @TransactionId), 0)
    SET @NetAmount = @OffsetAmount + @Amount

    IF (@OffsetAmount = 0)
    BEGIN
        RETURN @Amount
    END
    ELSE IF (@Amount < 0 AND @NetAmount > 0)
    BEGIN
        RETURN 0
    END

    RETURN @NetAmount

END
