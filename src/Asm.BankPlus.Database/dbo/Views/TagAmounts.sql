CREATE VIEW [dbo].[TagAmounts] AS
    SELECT t.AccountId, tt.TagId, tt.[Name], t.TransactionTypeId, SUM(t.Amount) as Total
    FROM TransactionsTags tt
    INNER JOIN [Transaction] t ON tt.TransactionId = t.TransactionId
    GROUP BY t.AccountId, tt.TagId, tt.[Name], t.TransactionTypeId