CREATE VIEW [dbo].[TransactionsTags] AS
WITH PivotTags AS (
SELECT tttID as TransactionId, TagId
FROM
(SELECT ttt.TransactionId as tttID, Tag1Id,Tag2Id,Tag3Id,Tag4Id FROM TransactionTransactionTag ttt INNER JOIN TagRelationships t ON ttt.TransactionTagId = t.Tag1Id) t2
UNPIVOT
 (TagId FOR Tag IN (Tag1Id,Tag2id,Tag3id,Tag4Id)) as UnP
)
SELECT p.TransactionId, p.TagId, t.[Name] FROM
PivotTags p
INNER JOIN TransactionTag t ON p.TagId = t.TransactionTagId