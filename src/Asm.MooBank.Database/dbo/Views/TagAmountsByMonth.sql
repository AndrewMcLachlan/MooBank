CREATE VIEW [dbo].[TagAmountsByMonth] AS
    SELECT t.AccountId, tt.TagId, tt.[Name], t.TransactionTypeId, ty.[Description], SUM(t.Amount) as Total, DATEPART(M, t.TransactionTime) as [Month], DATEPART(YEAR, t.TransactionTime) as [Year]
    FROM TransactionsTags tt
    INNER JOIN [Transaction] t ON tt.TransactionId = t.TransactionId
    INNER JOIN [TransactionType] ty ON t.TransactionTypeId = ty.TransactionTypeId
    GROUP BY t.AccountId, tt.TagId, tt.[Name], t.TransactionTypeId, ty.[Description], DATEPART(M, t.TransactionTime), DATEPART(YEAR, t.TransactionTime)
