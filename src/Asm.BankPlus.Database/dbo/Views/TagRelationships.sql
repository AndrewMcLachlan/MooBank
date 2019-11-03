CREATE VIEW [dbo].[TagRelationships] AS

SELECT tt1.TransactionTagId as Tag1Id, tt1.Name as Tag1Name, tt2.TransactionTagId as Tag2Id, tt2.[Name] as Tag2Name, tt3.TransactionTagId as Tag3Id, tt3.[Name] as Tag3Name, tt4.TransactionTagId as Tag4Id, tt4.[Name] as Tag4Name
FROM TransactionTag tt1
LEFT JOIN TransactionTagTransactionTag tttt1 ON tt1.TransactionTagId = tttt1.PrimaryTransactionTagId
LEFT JOIN TransactionTag tt2 ON tttt1.SecondaryTransactionTagId = tt2.TransactionTagId
LEFT JOIN TransactionTagTransactionTag tttt2 ON tt2.TransactionTagId = tttt2.PrimaryTransactionTagId
LEFT JOIN TransactionTag tt3 ON tttt2.SecondaryTransactionTagId = tt3.TransactionTagId
LEFT JOIN TransactionTagTransactionTag tttt3 ON tt3.TransactionTagId = tttt3.PrimaryTransactionTagId
LEFT JOIN TransactionTag tt4 ON tttt3.SecondaryTransactionTagId = tt4.TransactionTagId