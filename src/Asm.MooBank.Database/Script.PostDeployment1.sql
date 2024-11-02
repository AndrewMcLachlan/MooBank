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
MERGE [AccountType] AS TARGET USING (SELECT 1 as AccountTypeId, 'Transaction' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

MERGE [AccountType] AS TARGET USING (SELECT 2 as AccountTypeId, 'Savings' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

MERGE [AccountType] AS TARGET USING (SELECT 3 as AccountTypeId, 'Credit' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

MERGE [AccountType] AS TARGET USING (SELECT 4 as AccountTypeId, 'Mortgage' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

MERGE [AccountType] AS TARGET USING (SELECT 5 as AccountTypeId, 'Superannuation' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

MERGE [AccountType] AS TARGET USING (SELECT 6 as AccountTypeId, 'Investment' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

MERGE [AccountType] AS TARGET USING (SELECT 7 as AccountTypeId, 'Loan' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.AccountTypeId, SOURCE.[Description]);

-- Controller
MERGE [Controller] AS TARGET USING (SELECT 0 as Id, 'Manual' as [Type]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Type] <> SOURCE.[Type] THEN UPDATE SET Target.[Type] = SOURCE.[Type]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Type]);

MERGE [Controller] AS TARGET USING (SELECT 1 as Id, 'Virtual' as [Type]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Type] <> SOURCE.[Type] THEN UPDATE SET Target.[Type] = SOURCE.[Type]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Type]);

MERGE [Controller] AS TARGET USING (SELECT 2 as Id, 'Import' as [Type]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Type] <> SOURCE.[Type] THEN UPDATE SET Target.[Type] = SOURCE.[Type]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Type]);

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
MERGE ImporterType AS TARGET USING (SELECT 'Asm.MooBank.Institution.Ing.Importers.IngImporter, Asm.MooBank.Institution.Ing' as [Type], 'ING' as [Name]) AS SOURCE
ON (TARGET.[Type] = SOURCE.[Type])
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Type], SOURCE.[Name]);

MERGE ImporterType AS TARGET USING (SELECT 'Asm.MooBank.Institution.AustralianSuper.Importers.Importer, Asm.MooBank.Institution.AustralianSuper' as [Type], 'AustralianSuper' as [Name]) AS SOURCE
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

MERGE InstitutionType AS TARGET USING (SELECT 3 as Id, 'Broker' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 4 as Id, 'Credit Union' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 5 as Id, 'Building Society' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 6 as Id, 'Investment Fund' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 7 as Id, 'Government' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE InstitutionType AS TARGET USING (SELECT 8 as Id, 'Other' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

-- Family
MERGE Family as TARGET USING (SELECT 'DB1A117B-84A9-4F15-B6C2-6BD959B9BAF7' as Id, 'McLachlan' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);
