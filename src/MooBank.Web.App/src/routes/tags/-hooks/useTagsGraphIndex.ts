import { useMemo } from "react";

import type { TagGraph, TagGraphNode } from "api/types.gen";

export interface TagsGraphIndex {
    byId: Map<number, TagGraphNode>;
    childrenOf: Map<number, number[]>;
    parentsOf: Map<number, number[]>;
    roots: number[];
}

const compareByName = (byId: Map<number, TagGraphNode>) =>
    (a: number, b: number) => {
        const an = byId.get(a)?.name ?? "";
        const bn = byId.get(b)?.name ?? "";
        return an.localeCompare(bn, undefined, { sensitivity: "base" });
    };

export const buildTagsGraphIndex = (graph: TagGraph): TagsGraphIndex => {
    const byId = new Map<number, TagGraphNode>();
    for (const node of graph.nodes) {
        byId.set(node.id, node);
    }

    const childrenOf = new Map<number, number[]>();
    const parentsOf = new Map<number, number[]>();

    for (const node of graph.nodes) {
        childrenOf.set(node.id, []);
        parentsOf.set(node.id, []);
    }

    for (const edge of graph.edges) {
        if (!byId.has(edge.parentId) || !byId.has(edge.childId)) continue;
        childrenOf.get(edge.parentId)!.push(edge.childId);
        parentsOf.get(edge.childId)!.push(edge.parentId);
    }

    const cmp = compareByName(byId);
    for (const list of childrenOf.values()) list.sort(cmp);
    for (const list of parentsOf.values()) list.sort(cmp);

    const roots = graph.nodes
        .filter((n) => (parentsOf.get(n.id)?.length ?? 0) === 0)
        .map((n) => n.id)
        .sort(cmp);

    return { byId, childrenOf, parentsOf, roots };
};

export const useTagsGraphIndex = (graph: TagGraph | undefined): TagsGraphIndex | undefined =>
    useMemo(() => (graph ? buildTagsGraphIndex(graph) : undefined), [graph]);
