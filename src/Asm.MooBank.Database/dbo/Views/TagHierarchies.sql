CREATE VIEW [dbo].[TagHierarchies] AS

WITH  cte AS (
    SELECT tt.TransactionTagId as OGID, NULL as ChildId, tt.*, CAST(tt.Name as VARCHAR(MAX)) as ChildName, CAST(tt.TransactionTagId as VARCHAR(MAX)) as TagIds
    FROM TransactionTag tt
    INNER JOIN TransactionTagTransactionTag tttt ON tt.TransactionTagId = tttt.PrimaryTransactionTagId
    --LEFT  JOIN TransactionTagTransactionTag tttt2 ON tt.TransactionTagId = tttt2.SecondaryTransactionTagId
    --WHERE tttt2.SecondaryTransactionTagId IS NULL
    UNION ALL
    SELECT cte.OGID, cte.TransactionTagId as ChildId, tt.*, CAST(tt.Name + ', ' + cte.ChildName as VARCHAR(MAX)), CAST(tt.TransactionTagId as VARCHAR(MAX)) +',' + cte.TagIds
    FROM TransactionTag tt
    INNER JOIN TransactionTagTransactionTag tttt ON tt.TransactionTagId = tttt.SecondaryTransactionTagId
    INNER JOIN cte ON cte.TransactionTagId = tttt.PrimaryTransactionTagId
  ),
  cte2 AS (
    SELECT DISTINCT cte.OGID as ID, cte.TagIds FROM cte
    LEFT JOIN cte cte2 ON cte.TransactionTagId = cte2.ChildId
    WHERE cte2.TransactionTagId IS NULL
  ),
  cte3 AS (
    SELECT ID, CAST(splitTags.value as int) as ParentID, splitTags.ordinal as Ordinal FROM cte2
    CROSS APPLY STRING_SPLIT(cte2.TagIds, ',', 1) splitTags
  )
  SELECT * FROM cte3 WHERE ID <> ParentID