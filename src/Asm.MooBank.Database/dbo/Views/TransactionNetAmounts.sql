CREATE VIEW [dbo].[TransactionNetAmounts] AS
    SELECT
        *,
        [dbo].TransactionNetAmount(t.TransactionId, t.Amount) AS NetAmount
    FROM
        [dbo].[Transaction] t
