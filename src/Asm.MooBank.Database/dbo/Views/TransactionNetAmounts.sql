CREATE VIEW [dbo].[TransactionNetAmounts]
AS
    SELECT
    [TransactionId],
    [TransactionReference],
    [AccountId],
    [AccountHolderId],
    [TransactionTypeId],
    [Amount],
    [dbo].TransactionNetAmount(t.TransactionTypeId, t.TransactionId, t.Amount) AS NetAmount,
    [Description],
    [Location],
    [Reference],
    [PurchaseDate],
    [TransactionTime],
    [Notes],
    [Extra],
    [ExcludeFromReporting],
    [Created],
    [Source]
    FROM
        [dbo].[Transaction] t
GO
