import type { Tag } from "api/types.gen";

export const getParents = (allTags: Tag[], tagId: number): Tag[] =>
    allTags.filter(t => t.tags?.some(child => child.id === tagId));

const walk = (startId: number, edges: Map<number, number[]>): Set<number> => {
    const visited = new Set<number>();
    const pending = [...(edges.get(startId) ?? [])];

    while (pending.length > 0) {
        const id = pending.pop()!;
        if (id === startId || visited.has(id)) continue;
        visited.add(id);
        pending.push(...(edges.get(id) ?? []));
    }

    return visited;
};

const buildEdges = (allTags: Tag[]) => {
    const childrenOf = new Map<number, number[]>();
    const parentsOf = new Map<number, number[]>();

    for (const parent of allTags) {
        for (const child of parent.tags ?? []) {
            childrenOf.set(parent.id, [...(childrenOf.get(parent.id) ?? []), child.id]);
            parentsOf.set(child.id, [...(parentsOf.get(child.id) ?? []), parent.id]);
        }
    }

    return { childrenOf, parentsOf };
};

export const getDescendantIds = (allTags: Tag[], tagId: number): Set<number> =>
    walk(tagId, buildEdges(allTags).childrenOf);

export const getAncestorIds = (allTags: Tag[], tagId: number): Set<number> =>
    walk(tagId, buildEdges(allTags).parentsOf);

export const getRelatableTags = (allTags: Tag[], tagId: number): Tag[] => {
    const { childrenOf, parentsOf } = buildEdges(allTags);
    const related = new Set([tagId, ...walk(tagId, parentsOf), ...walk(tagId, childrenOf)]);
    return allTags.filter(t => !related.has(t.id));
};
