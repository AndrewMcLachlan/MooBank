/*
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.
 Example:      :setvar TableName MyTable
               SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*/

-- Account Type
MERGE AccountType AS TARGET USING (SELECT 1 as AccountTypeId, 'Transaction' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (1, SOURCE.[Description]);

MERGE AccountType AS TARGET USING (SELECT 2 as AccountTypeId, 'Savings' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (2, SOURCE.[Description]);

MERGE AccountType AS TARGET USING (SELECT 3 as AccountTypeId, 'Credit' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (3, SOURCE.[Description]);

MERGE AccountType AS TARGET USING (SELECT 4 as AccountTypeId, 'Mortgage' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (4, SOURCE.[Description]);

MERGE AccountType AS TARGET USING (SELECT 5 as AccountTypeId, 'Superannuation' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (5, SOURCE.[Description]);

--Account Controller
MERGE AccountController AS TARGET USING (SELECT 0 as AccountControllerId, 'Manual' as [Type]) AS SOURCE
ON (TARGET.AccountControllerId = SOURCE.AccountControllerId)
WHEN MATCHED AND TARGET.[Type] <> SOURCE.[Type] THEN UPDATE SET Target.[Type] = SOURCE.[Type]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (0, SOURCE.[Type]);

MERGE AccountController AS TARGET USING (SELECT 1 as AccountControllerId, 'VirtualAccount' as [Type]) AS SOURCE
ON (TARGET.AccountControllerId = SOURCE.AccountControllerId)
WHEN MATCHED AND TARGET.[Type] <> SOURCE.[Type] THEN UPDATE SET Target.[Type] = SOURCE.[Type]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (1, SOURCE.[Type]);

MERGE AccountController AS TARGET USING (SELECT 2 as AccountControllerId, 'Import' as [Type]) AS SOURCE
ON (TARGET.AccountControllerId = SOURCE.AccountControllerId)
WHEN MATCHED AND TARGET.[Type] <> SOURCE.[Type] THEN UPDATE SET Target.[Type] = SOURCE.[Type]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (2, SOURCE.[Type]);

-- Transaction Type
MERGE TransactionType AS TARGET USING (SELECT 1 as TransactionTypeId, 'Credit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (1, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 2 as TransactionTypeId, 'Debit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (2, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 3 as TransactionTypeId, 'BalanceAdjustment' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (3, SOURCE.[Description]);


-- Importer Types
MERGE ImporterType AS TARGET USING (SELECT 'Asm.BankPlus.Services.Importers.IngImporter, Asm.BankPlus.Services' as [Type], 'ING' as [Name]) AS SOURCE
ON (TARGET.[Type] = SOURCE.[Type])
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Type], SOURCE.[Name]);

