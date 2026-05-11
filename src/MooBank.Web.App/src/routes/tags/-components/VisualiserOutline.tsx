import { useMemo } from "react";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";
import { VisualiserOutlineRow } from "./VisualiserOutlineRow";

const MAX_DEPTH = 8;

interface Props {
    index: TagsGraphIndex;
    expanded: Set<string>;
    focusId: number | null;
    search: string;
    onToggle: (pathKey: string) => void;
    onFocus: (id: number) => void;
    onMatchCountChange?: (count: number) => void;
}

const buildMatchSet = (index: TagsGraphIndex, term: string): Set<number> => {
    if (!term) return new Set();
    const lower = term.toLowerCase();
    const result = new Set<number>();
    for (const node of index.byId.values()) {
        if (node.name.toLowerCase().includes(lower)) result.add(node.id);
    }
    return result;
};

const buildPathToMatchSet = (index: TagsGraphIndex, matches: Set<number>): Set<number> => {
    if (matches.size === 0) return new Set();
    const result = new Set<number>(matches);
    const stack = Array.from(matches);
    while (stack.length > 0) {
        const id = stack.pop()!;
        const parents = index.parentsOf.get(id) ?? [];
        for (const p of parents) {
            if (!result.has(p)) {
                result.add(p);
                stack.push(p);
            }
        }
    }
    return result;
};

export const VisualiserOutline: React.FC<Props> = ({
    index, expanded, focusId, search, onToggle, onFocus, onMatchCountChange,
}) => {
    const matches = useMemo(() => buildMatchSet(index, search), [index, search]);
    const visible = useMemo(() => buildPathToMatchSet(index, matches), [index, matches]);
    const isFiltering = search.length > 0;

    if (onMatchCountChange) onMatchCountChange(matches.size);

    const renderNode = (id: number, ancestors: ReadonlySet<number>, parentPath: string, level: number): React.ReactNode => {
        if (level >= MAX_DEPTH) return null;

        const node = index.byId.get(id);
        if (!node) return null;

        if (isFiltering && !visible.has(id)) return null;

        const isCycle = ancestors.has(id);
        const pathKey = parentPath === "" ? `${id}` : `${parentPath}/${id}`;

        const children = index.childrenOf.get(id) ?? [];
        const hasChildren = children.length > 0 && !isCycle;
        const isOpen = isFiltering ? matches.size > 0 : expanded.has(pathKey);

        const nextAncestors = new Set(ancestors);
        nextAncestors.add(id);

        return (
            <div key={pathKey} role="group">
                <VisualiserOutlineRow
                    node={node}
                    pathKey={pathKey}
                    level={level}
                    isFocused={focusId === node.id}
                    isExpanded={isOpen}
                    hasChildren={hasChildren}
                    isCycle={isCycle}
                    searchTerm={search}
                    onToggle={onToggle}
                    onFocus={onFocus}
                />
                {hasChildren && isOpen && children.map((cid) => renderNode(cid, nextAncestors, pathKey, level + 1))}
            </div>
        );
    };

    return (
        <div role="tree" aria-label="Tag hierarchy" className="visualiser-outline">
            {index.roots.map((rid) => renderNode(rid, new Set(), "", 0))}
        </div>
    );
};
