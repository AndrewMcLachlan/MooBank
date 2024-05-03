CREATE VIEW [dbo].[TagHierarchies] AS

WITH  cte AS (
    SELECT tt.[Id] as OGID, NULL as ChildId, tt.*, CAST(tt.Name as VARCHAR(MAX)) as ChildName, CAST(tt.[Id] as VARCHAR(MAX)) as TagIds
    FROM [Tag] tt
    INNER JOIN [TagTag] tttt ON tt.[Id] = tttt.[PrimaryTagId]
    --LEFT  JOIN TransactionTagTransactionTag tttt2 ON tt.TransactionTagId = tttt2.SecondaryTransactionTagId
    --WHERE tttt2.SecondaryTransactionTagId IS NULL
    UNION ALL
    SELECT cte.OGID, cte.Id as ChildId, tt.*, CAST(tt.Name + ', ' + cte.ChildName as VARCHAR(MAX)), CAST(tt.[Id] as VARCHAR(MAX)) +',' + cte.TagIds
    FROM [Tag] tt
    INNER JOIN [TagTag] tttt ON tt.[Id] = tttt.[SecondaryTagId]
    INNER JOIN cte ON cte.Id = tttt.[PrimaryTagId]
  ),
  cte2 AS (
    SELECT DISTINCT cte.OGID as ID, cte.TagIds FROM cte
    LEFT JOIN cte cte2 ON cte.[Id] = cte2.ChildId
    WHERE cte2.[Id] IS NULL
  ),
  cte3 AS (
    SELECT ID, CAST(splitTags.value as int) as ParentID, splitTags.ordinal as Ordinal FROM cte2
    CROSS APPLY STRING_SPLIT(cte2.TagIds, ',', 1) splitTags
  )
  SELECT * FROM cte3 WHERE ID <> ParentID