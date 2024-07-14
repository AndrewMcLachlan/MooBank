CREATE FUNCTION [dbo].[TransactionNetAmount]
(
    @TransactionId UNIQUEIDENTIFIER NULL,
    @Amount DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @OffsetAmount DECIMAL(10,2)
    DECLARE @NetAmount DECIMAL(10,2)

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
