-- One-off script to update existing forecast plans to use IncomeCorrelated mode
UPDATE dbo.ForecastPlan
SET OutgoingStrategy = JSON_MODIFY(COALESCE(OutgoingStrategy, '{"version":1,"lookbackMonths":12}'), '$.mode', 'IncomeCorrelated')
WHERE IsArchived = 0;
