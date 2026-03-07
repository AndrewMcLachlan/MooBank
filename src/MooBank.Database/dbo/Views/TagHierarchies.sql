CREATE VIEW [dbo].[TagHierarchies] AS

WITH  cte AS (
    SELECT t.[Id] as OGID, NULL as ChildId, t.Id, t.Name, CAST(t.Name as VARCHAR(MAX)) as ChildName, CAST(t.[Id] as VARCHAR(MAX)) as TagIds
    FROM [Tag] t
    INNER JOIN [TagTag] tt ON t.[Id] = tt.[PrimaryTagId]
    UNION ALL
    SELECT cte.OGID, cte.Id as ChildId, t.Id, t.Name, CAST(t.Name + ', ' + cte.ChildName as VARCHAR(MAX)), CAST(t.[Id] as VARCHAR(MAX)) +',' + cte.TagIds
    FROM [Tag] t
    INNER JOIN [TagTag] tt ON t.[Id] = tt.[SecondaryTagId]
    INNER JOIN cte ON cte.Id = tt.[PrimaryTagId]
  ),
  cte2 AS (
    SELECT DISTINCT cte.OGID as ID, cte.TagIds FROM cte
    LEFT JOIN cte cte2 ON cte.[Id] = cte2.ChildId
    WHERE cte2.[Id] IS NULL
  ),
  cte3 AS (
    SELECT ID, CAST(splitags.value as int) as ParentID, splitags.ordinal as Ordinal FROM cte2
    CROSS APPLY STRING_SPLIT(cte2.TagIds, ',', 1) splitags
  )
  SELECT ID, ParentID, Ordinal FROM cte3 WHERE ID <> ParentID