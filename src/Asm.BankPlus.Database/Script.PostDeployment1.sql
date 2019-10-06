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
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (1, 'Transaction');

MERGE AccountType AS TARGET USING (SELECT 2 as AccountTypeId, 'Savings' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (2, 'Savings');

MERGE AccountType AS TARGET USING (SELECT 3 as AccountTypeId, 'Credit' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (3, 'Credit');

MERGE AccountType AS TARGET USING (SELECT 4 as AccountTypeId, 'Mortgage' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (4, 'Mortgage');

MERGE AccountType AS TARGET USING (SELECT 5 as AccountTypeId, 'Superannuation' as [Description]) AS SOURCE
ON (TARGET.AccountTypeId = SOURCE.AccountTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (5, 'Superannuation');


-- Transaction Type
MERGE TransactionType AS TARGET USING (SELECT 1 as TransactionTypeId, 'Credit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (1, 'Credit');

MERGE TransactionType AS TARGET USING (SELECT 2 as TransactionTypeId, 'Debit' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (2, 'Debit');

MERGE TransactionType AS TARGET USING (SELECT 3 as TransactionTypeId, 'BalanceAdjustment' as [Description]) AS SOURCE
ON (TARGET.TransactionTypeId = SOURCE.TransactionTypeId)
WHEN MATCHED AND TARGET.[Description] <> SOURCE.[Description] THEN UPDATE SET Target.[Description] = SOURCE.[Description]
WHEN NOT MATCHED BY TARGET THEN INSERT VALUES (3, 'BalanceAdjustment');

/*
IF ((SELECT COUNT(*)FROM TransactionCategory) = 0)
BEGIN
INSERT INTO TransactionCategory ([Description]) VALUES ('Living Expense') --1
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Utilities', 1) --2
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Tax', 1) --3
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Child', 1) --4
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Insurance', 1) --5
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Transport', 1) --6
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Clothes', 1) --6
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Home Maintenance', 1) --7
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Home Improvement', 1) --7

INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Elecricity', 2)
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Water', 2)
INSERT INTO TransactionCategory ([Description], ParentCategoryId) VALUES ('Rates', 1)
END
*/