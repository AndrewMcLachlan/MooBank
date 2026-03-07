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


MERGE TransactionSubType AS TARGET USING (SELECT 1 as Id, 'Recurring' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 2 as Id, 'BalanceAdjustment' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 3 as Id, 'OpeningBalance' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);


MERGE TransactionSubType AS TARGET USING (SELECT 5 as Id, 'Visa' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 6 as Id, 'MasterCard' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 7 as Id, 'Direct Debit' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 8 as Id, 'EFTPOS' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 9 as Id, 'ATM' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 10 as Id, 'OSKO' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE TransactionSubType AS TARGET USING (SELECT 11 as Id, 'BPAY' as [Description]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

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

MERGE ImporterType AS TARGET USING (SELECT 'Asm.MooBank.Institution.Macquarie.Importers.MacquarieImporter, Asm.MooBank.Institution.Macquarie' as [Type], 'Macquarie' as [Name]) AS SOURCE
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

MERGE InstitutionType AS TARGET USING (SELECT 9 as Id, 'Utility' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);


-- Utility Type
MERGE [utilities].UtilityType AS TARGET USING (SELECT 1 as Id, 'Electricity' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE [utilities].UtilityType AS TARGET USING (SELECT 2 as Id, 'Gas' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE [utilities].UtilityType AS TARGET USING (SELECT 3 as Id, 'Water' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE [utilities].UtilityType AS TARGET USING (SELECT 4 as Id, 'Phone' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE [utilities].UtilityType AS TARGET USING (SELECT 5 as Id, 'Internet' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

MERGE [utilities].UtilityType AS TARGET USING (SELECT 6 as Id, 'Other' as [Name]) AS SOURCE
ON (TARGET.Id = SOURCE.Id)
WHEN MATCHED AND TARGET.[Name] <> SOURCE.[Name] THEN UPDATE SET Target.[Name] = SOURCE.[Name]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.[Id], SOURCE.[Name]);

-- Set some default institutions
IF ((SELECT COUNT(*) FROM [dbo].[Institution]) = 0)
BEGIN
SET IDENTITY_INSERT [dbo].[Institution] ON
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (1, N'ING', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (2, N'AustralianSuper', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (3, N'Australian Retirement Trust', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (4, N'Scottish Widows', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (5, N'CMC', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (165, N'AMP Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (166, N'ANZ', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (167, N'Australian Military Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (168, N'Auswide Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (169, N'AWA Alliance Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (170, N'Bank of us', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (171, N'Bank Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (172, N'Bank of Melbourne', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (173, N'Bank of Queensland', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (174, N'BankSA', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (175, N'Bankwest', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (176, N'BankVic', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (177, N'BDCU Alliance Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (178, N'Bendigo and Adelaide Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (179, N'Border Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (180, N'BOQ Specialist', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (181, N'CIRCLE Alliance Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (182, N'Commonwealth Bank of Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (183, N'Beyond Bank Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (184, N'Regional Australia Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (185, N'Community Sector Banking', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (186, N'Defence Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (187, N'Delphi Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (188, N'Esanda', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (189, N'Firefighters Mutual Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (190, N'G&C Mutual Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (191, N'Greater Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (192, N'Heritage Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (193, N'Hume Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (194, N'IMB Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (195, N'Judo Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (196, N'Macquarie Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (197, N'ME Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (198, N'MyState Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (199, N'NAB', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (200, N'P&N Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (201, N'Police Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (202, N'QBANK', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (203, N'Qudos Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (204, N'RACQ Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (205, N'Reliance Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (206, N'RSL Money', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (207, N'Rural Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (208, N'SERVICE ONE Alliance Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (209, N'St.George Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (210, N'Suncorp Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (211, N'Teachers Mutual Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (212, N'The Rock', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (213, N'Tyro Payments', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (214, N'Victoria Teachers Mutual Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (215, N'Ubank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (216, N'UniBank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (217, N'Unity Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (218, N'Up Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (219, N'Westpac Banking Corporation', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (220, N'Volt Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (221, N'86 400 Bank', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (222, N'Arab Bank Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (223, N'Bank of China (Australia)', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (224, N'Bank of Sydney', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (225, N'HSBC Bank Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (226, N'Citi Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (227, N'Rabobank Australia', 1)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (228, N'Alcoa of Australia Retirement Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (229, N'Acclaim Wealth', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (230, N'AMP Super Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (231, N'ASGARD Independence Plan Division Two', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (232, N'Australian Defence Force Superannuation Scheme', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (233, N'Australian Ethical Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (234, N'Australian Food Super (formerly AMIST)', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (235, N'Avanteos Superannuation Trust', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (236, N'AvSuper Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (237, N'AvWrap Retirement Service', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (238, N'Aware Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (239, N'BUSSQ', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (240, N'Care Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (241, N'Centric Super Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (242, N'Challenger Retirement Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (243, N'ClearView Retirement Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (244, N'Colonial First State FirstChoice', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (245, N'CommInsure Corporate Insurance Superannuation Trust', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (246, N'Cbus Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (247, N'Crescent Wealth Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (248, N'NSW Fire Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (249, N'CSC (Commonwealth Superannuation Corporation)', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (250, N'DPM Retirement Service', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (251, N'Equip Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (252, N'Commonwealth Essential Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (253, N'Fiducian Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (254, N'Fire and Emergency Services Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (255, N'First Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (256, N'Future Super Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (257, N'Goldman Sachs & JBWere Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (258, N'Grosvenor Pirie Master Superannuation Fund Series 2', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (259, N'GuildSuper', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (260, N'HESTA', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (261, N'Hostplus', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (262, N'HUB24 Super Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (263, N'IOOF Portfolio Service Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (264, N'legalsuper', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (265, N'Brighter Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (266, N'Lifefocus Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (267, N'Vision Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (268, N'Active Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (269, N'Macquarie Superannuation Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (270, N'Mason Stevens Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (271, N'Meat Industry Employees Superannuation Fund (MIESF)', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (272, N'Mercer Portfolio Service Superannuation Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (273, N'Mercer Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (274, N'Military Superannuation & Benefits Fund No 1', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (275, N'Mine Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (276, N'MLC Super Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (277, N'MLC Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (278, N'National Mutual Retirement Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (279, N'NESS Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (280, N'Netwealth Superannuation Master Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (281, N'NGS Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (282, N'Oasis Superannuation Master Trust', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (283, N'OneSuper', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (284, N'Perpetual Super Wrap', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (285, N'Perpetual WealthFocus Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (286, N'Perpetual’s Select Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (287, N'Personal Choice Private Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (288, N'Platformplus Super Wrap', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (289, N'Praemium SMA Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (290, N'Premiumchoice Retirement Service', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (291, N'Prime Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (292, N'Public Sector Superannuation Accumulation Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (293, N'Public Sector Superannuation Scheme', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (294, N'Qantas Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (295, N'Rei Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (296, N'REST Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (297, N'Retirement Portfolio Service (OnePath and ANZ Super)', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (298, N'Russell Investments Master Trust', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (299, N'smartMonday', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (300, N'Spirit Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (301, N'Star Portfolio Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (302, N'Super Retirement Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (303, N'Super Simplifier', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (304, N'SuperTrace Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (305, N'Symetry Personal Retirement Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (306, N'TelstraSuper', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (307, N'The Bendigo Superannuation Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (308, N'Tidswell Master Superannuation Plan', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (309, N'TWUSUPER', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (310, N'Ultimate Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (311, N'Unisuper', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (312, N'Vanguard Super', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (313, N'Wealth Personal Superannuation and Pension Fund (North and AMP)', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (314, N'Zurich Master Superannuation Fund', 2)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (315, N'Amscot Stockbroking', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (316, N'Argonaut Securities ', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (317, N'Ascot Securities ', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (318, N'Australian Investment Exchange (AUSIEX)', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (319, N'Bell Direct', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (320, N'Bell Potter Securities', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (321, N'Bridges Financial Services ', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (322, N'Burrell Stockbroking', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (323, N'Canaccord Genuity Financial', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (324, N'CCZ Equities Australia', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (325, N'Commonwealth Securities', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (326, N'Desktop Broker', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (327, N'Euroz Hartleys', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (328, N'Evans and Partners ', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (329, N'Finclear', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (330, N'FNZ Securities', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (331, N'Interactive Brokers Australia ', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (332, N'Joseph Palmer & Sons', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (333, N'Macquarie Equities', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (334, N'Morgan Stanley Wealth Management', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (335, N'Morgans Financial', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (336, N'Morrison Securities', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (337, N'nabtrade', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (338, N'OpenMarkets', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (339, N'Ord Minnett', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (340, N'Phillip Capital', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (341, N'Sequoia Direct', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (342, N'Shaw and Partners', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (343, N'State One Stockbroking', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (344, N'Taylor Collison', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (345, N'Webull', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (346, N'Wilsons Advisory and Stockbroking', 3)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (347, N'People''s Choice Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (348, N'Bananacoast Community Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (349, N'CAPE Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (350, N'Central Coast Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (351, N'Central Murray Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (352, N'Central West Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (353, N'Coastline Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (354, N'Community Alliance Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (355, N'Community First Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (356, N'Great Southern Bank', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (357, N'Credit Union SA ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (358, N'Dnister Ukrainian Credit Co-operative', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (359, N'Nexus Mutual', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (360, N'Family First Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (361, N'Fire Service Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (362, N'Firefighters & Affiliates Credit Co-operative', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (363, N'First Choice Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (364, N'First Option Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (365, N'Ford Co-operative Credit Society', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (366, N'Gateway Bank', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (367, N'Goulburn Murray Credit Union Co-operative', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (368, N'Heritage Isle Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (369, N'Holiday Coast Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (370, N'Horizon Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (371, N'Hunter United Employees'' Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (372, N'Laboratories Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (373, N'Lithuanian Co-operative Credit Society "Talka"', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (374, N'Lysaght Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (375, N'MacArthur Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (376, N'Macquarie Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (377, N'MCU ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (378, N'My Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (379, N'Northern Inland Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (380, N'Nova Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (381, N'Orange Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (382, N'Police Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (383, N'Queensland Country Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (384, N'Queenslanders Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (385, N'MOVE', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (386, N'Select Encompass Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (387, N'South West Slopes Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (388, N'Southern Cross Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (389, N'South-West Credit Union Co-Operative', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (390, N'Summerland Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (391, N'Sydney Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (392, N'The Broken Hill Community Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (393, N'The Capricornian ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (394, N'The Gympie Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (395, N'Traditional Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (396, N'Transport Mutual Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (397, N'Warwick Credit Union ', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (398, N'BankWAW', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (399, N'Woolworths Employees'' Credit Union', 4)
INSERT [dbo].[Institution] ([Id], [Name], [InstitutionTypeId]) VALUES (401, N'Other', 8)
SET IDENTITY_INSERT [dbo].[Institution] OFF
END

-- Account Scope Mode (for Forecast Plans)
MERGE [AccountScopeMode] AS TARGET USING (SELECT 0 as Id, 'AllAccounts' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [AccountScopeMode] AS TARGET USING (SELECT 1 as Id, 'SelectedAccounts' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

-- Starting Balance Mode (for Forecast Plans)
MERGE [StartingBalanceMode] AS TARGET USING (SELECT 0 as Id, 'CalculatedCurrent' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [StartingBalanceMode] AS TARGET USING (SELECT 1 as Id, 'ManualAmount' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

-- Planned Item Type (for Forecast Plans)
MERGE [PlannedItemType] AS TARGET USING (SELECT 0 as Id, 'Expense' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [PlannedItemType] AS TARGET USING (SELECT 1 as Id, 'Income' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

-- Planned Item Date Mode (for Forecast Plans)
MERGE [PlannedItemDateMode] AS TARGET USING (SELECT 0 as Id, 'FixedDate' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [PlannedItemDateMode] AS TARGET USING (SELECT 1 as Id, 'Schedule' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [PlannedItemDateMode] AS TARGET USING (SELECT 2 as Id, 'FlexibleWindow' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

-- Allocation Mode (for Forecast Plans - Flexible Window)
MERGE [AllocationMode] AS TARGET USING (SELECT 0 as Id, 'EvenlySpread' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [AllocationMode] AS TARGET USING (SELECT 1 as Id, 'AllAtEnd' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

-- Schedule Frequency (for Forecast Plans - Schedule)
MERGE [ScheduleFrequency] AS TARGET USING (SELECT 1 as Id, 'Daily' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [ScheduleFrequency] AS TARGET USING (SELECT 2 as Id, 'Weekly' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [ScheduleFrequency] AS TARGET USING (SELECT 3 as Id, 'Monthly' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [ScheduleFrequency] AS TARGET USING (SELECT 4 as Id, 'Yearly' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);

MERGE [ScheduleFrequency] AS TARGET USING (SELECT 5 as Id, 'Fortnightly' as [Description]) AS SOURCE
ON (TARGET.[Id] = SOURCE.Id)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (SOURCE.Id, SOURCE.[Description]);
