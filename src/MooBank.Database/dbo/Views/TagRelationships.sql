CREATE VIEW [dbo].[TagRelationships] AS

SELECT tt1.[Id] as Tag1Id, tt1.Name as Tag1Name, tt2.[Id] as Tag2Id, tt2.[Name] as Tag2Name, tt3.[Id] as Tag3Id, tt3.[Name] as Tag3Name, tt4.[Id] as Tag4Id, tt4.[Name] as Tag4Name
FROM [Tag] tt1
LEFT JOIN [TagTag] tttt1 ON tt1.[Id] = tttt1.[PrimaryTagId]
LEFT JOIN [Tag] tt2 ON tttt1.[SecondaryTagId] = tt2.[Id]
LEFT JOIN [TagTag] tttt2 ON tt2.[Id] = tttt2.[PrimaryTagId]
LEFT JOIN [Tag] tt3 ON tttt2.[SecondaryTagId] = tt3.[Id]
LEFT JOIN [TagTag] tttt3 ON tt3.[Id] = tttt3.[PrimaryTagId]
LEFT JOIN [Tag] tt4 ON tttt3.[SecondaryTagId] = tt4.[Id]