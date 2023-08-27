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
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.TransactionTypeId, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 2 as TransactionTypeId, 'Debit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.TransactionTypeId, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 3 as TransactionTypeId, 'RecurringCredit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.TransactionTypeId, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 4 as TransactionTypeId, 'RecurringDebit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.TransactionTypeId, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 5 as TransactionTypeId, 'BalanceAdjustmentCredit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.TransactionTypeId, SOURCE.[Description]);

MERGE TransactionType AS TARGET USING (SELECT 6 as TransactionTypeId, 'BalanceAdjustmentDebit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.TransactionTypeId, SOURCE.[Description]);


-- Schedule
MERGE Schedule AS TARGET USING (SELECT 1 as ScheduleId, 'Daily' as [Description]) AS SOURCE
ON (TARGET.ScheduleId = SOURCE.ScheduleId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.ScheduleId, SOURCE.[Description]);

MERGE Schedule AS TARGET USING (SELECT 2 as ScheduleId, 'Weekly' as [Description]) AS SOURCE
ON (TARGET.ScheduleId = SOURCE.ScheduleId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.ScheduleId, SOURCE.[Description]);

MERGE Schedule AS TARGET USING (SELECT 3 as ScheduleId, 'Monthly' as [Description]) AS SOURCE
ON (TARGET.ScheduleId = SOURCE.ScheduleId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.ScheduleId, SOURCE.[Description]);

-- Importer Types
MERGE ImporterType AS TARGET USING (SELECT 'Asm.MooBank.Services.Importers.IngImporter, Asm.MooBank.Services' as [Type], 'ING' as [Name]) AS SOURCE
ON (TARGET.[Type] = SOURCE.[Type])
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Type], SOURCE.[Name]);


-- Institution Type
MERGE InstitutionType AS TARGET USING (SELECT 1 as Id, 'Bank' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 2 as Id, 'Superannutation Fund' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 3 as Id, 'Share Trading' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);


-- Institution
MERGE Institution AS TARGET USING (SELECT 1 as Id, 'ING' as [Name], 1 as [InstitutionTypeId]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name], Target.[InstitutionTypeId] = SOURCE.[InstitutionTypeId]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name], SOURCE.[InstitutionTypeId]);

MERGE Institution AS TARGET USING (SELECT 2 as Id, 'AustralianSuper' as [Name], 2 as [InstitutionTypeId]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name], Target.[InstitutionTypeId] = SOURCE.[InstitutionTypeId]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name], SOURCE.[InstitutionTypeId]);

-- Family
MERGE Family as TARGET USING (SELECT 'DB1A117B-84A9-4F15-B6C2-6BD959B9BAF7' as Id, 'McLachlan' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

-- Account holder update
UPDATE AccountHolder SET FamilyId = 'DB1A117B-84A9-4F15-B6C2-6BD959B9BAF7'

-- Update Institution Account
UPDATE InstitutionAccount SET InstitutionId = 1 WHERE AccountId IN
('6b4ae4d9-d4ba-41f7-80e6-076863df9407',
'3740bb76-e226-4bce-a8ab-3203824a6fa9',
'1c42cea9-6b71-4e37-bced-8ef259e9e55e')

UPDATE InstitutionAccount SET InstitutionId = 2 WHERE AccountId IN
('5688057c-88eb-4b78-8041-821a3e3ec478')

-- Tag update
UPDATE Tag SET FamilyId = 'DB1A117B-84A9-4F15-B6C2-6BD959B9BAF7'

-- Budget update
UPDATE Budget SET FamilyId = 'DB1A117B-84A9-4F15-B6C2-6BD959B9BAF7'