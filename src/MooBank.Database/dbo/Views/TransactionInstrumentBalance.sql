-- Read-model for TransactionInstrument-derived values (Balance, LastTransaction).
-- Replaces the per-row scalar UDFs dbo.AccountBalance / dbo.LastTransaction, which
-- could not be inlined because scalar UDFs used in a computed column are excluded from
-- inlining (see https://aka.ms/sqludfinlining). Grounding on TransactionInstrument with a
-- LEFT JOIN guarantees one row per instrument, including instruments with no transactions
-- (Balance 0, LastTransaction NULL), matching the previous ISNULL / MAX-over-empty semantics.
CREATE VIEW [dbo].[TransactionInstrumentBalance]
AS
SELECT
    ti.[InstrumentId],
    CAST(ISNULL(SUM(CASE WHEN t.TransactionTypeId = 1 THEN t.Amount ELSE -ABS(t.Amount) END), 0) AS DECIMAL(12, 4)) AS [Balance],
    CAST(MAX(t.TransactionTime) AS DATE) AS [LastTransaction]
FROM [dbo].[TransactionInstrument] ti
LEFT JOIN [dbo].[Transaction] t ON t.[AccountId] = ti.[InstrumentId]
GROUP BY ti.[InstrumentId];
