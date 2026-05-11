import classNames from "classnames";
import { useEffect, useState } from "react";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";

const TRANSITION_MS = 220;

interface Props {
    index: TagsGraphIndex;
    focusId: number | null;
    onFocus: (id: number) => void;
}

const VIEW_W = 600;
const NODE_W = 84;
const NODE_H = 22;
const COL_GAP = 8;
const ROW_GAP = 8;
const PAD_X = 12;
const FOCUS_GAP_Y = 28;     // vertical gap between focus and the nearest parent/child row
const OUTER_PAD_Y = 12;

type NodeState = "visible" | "leaving" | "entering";

interface DisplayNode {
    id: number;
    x: number;
    y: number;
    label: string;
    colour: string | null;
    kind: "parent" | "focus" | "child";
    state: NodeState;
}

interface DisplayEdge {
    id: string;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    state: NodeState;
}

interface Layout {
    nodes: Omit<DisplayNode, "state">[];
    edges: Omit<DisplayEdge, "state">[];
    viewH: number;
}

const chipsPerRow = (): number => {
    const usable = VIEW_W - PAD_X * 2;
    const slot = NODE_W + COL_GAP;
    return Math.max(1, Math.floor((usable + COL_GAP) / slot));
};

const buildLayout = (forFocusId: number, index: TagsGraphIndex): Layout => {
    const focused = index.byId.get(forFocusId);
    if (!focused) return { nodes: [], edges: [], viewH: 0 };

    const parentIds = index.parentsOf.get(forFocusId) ?? [];
    const childIds = index.childrenOf.get(forFocusId) ?? [];

    const perRow = chipsPerRow();

    const parentRows = Math.max(1, Math.ceil(parentIds.length / perRow));
    const childRows = Math.max(1, Math.ceil(childIds.length / perRow));
    const parentHeight = parentIds.length === 0
        ? 0
        : FOCUS_GAP_Y + parentRows * NODE_H + (parentRows - 1) * ROW_GAP;
    const childHeight = childIds.length === 0
        ? 0
        : FOCUS_GAP_Y + childRows * NODE_H + (childRows - 1) * ROW_GAP;

    const focusY = OUTER_PAD_Y + parentHeight + NODE_H / 2;
    const viewH = OUTER_PAD_Y + parentHeight + NODE_H + childHeight + OUTER_PAD_Y;

    const nodes: Omit<DisplayNode, "state">[] = [];
    const edges: Omit<DisplayEdge, "state">[] = [];

    // Focus node
    const focusX = VIEW_W / 2;
    nodes.push({
        id: forFocusId,
        x: focusX,
        y: focusY,
        label: focused.name,
        colour: focused.colour as string | null,
        kind: "focus",
    });

    // Layout helper for a group of ids in rows above (-1) or below (+1) the focus
    const layoutGroup = (ids: number[], direction: -1 | 1, kind: "parent" | "child") => {
        const numRows = Math.ceil(ids.length / perRow);
        for (let r = 0; r < numRows; r++) {
            const start = r * perRow;
            const end = Math.min(start + perRow, ids.length);
            const inThisRow = end - start;
            const totalWidth = inThisRow * NODE_W + (inThisRow - 1) * COL_GAP;
            const rowStartX = (VIEW_W - totalWidth) / 2;
            const y = focusY + direction * (FOCUS_GAP_Y + r * (NODE_H + ROW_GAP) + NODE_H / 2);
            for (let i = 0; i < inThisRow; i++) {
                const id = ids[start + i];
                const node = index.byId.get(id);
                if (!node) continue;
                const x = rowStartX + i * (NODE_W + COL_GAP) + NODE_W / 2;
                nodes.push({ id, x, y, label: node.name, colour: node.colour as string | null, kind });
                // Edge between focus and this node
                const edgeId = kind === "parent" ? `${id}-${forFocusId}` : `${forFocusId}-${id}`;
                const x1 = kind === "parent" ? x : focusX;
                const y1 = kind === "parent" ? y + NODE_H / 2 : focusY + NODE_H / 2;
                const x2 = kind === "parent" ? focusX : x;
                const y2 = kind === "parent" ? focusY - NODE_H / 2 : y - NODE_H / 2;
                edges.push({ id: edgeId, x1, y1, x2, y2 });
            }
        }
    };

    layoutGroup(parentIds, -1, "parent");
    layoutGroup(childIds, 1, "child");

    return { nodes, edges, viewH };
};

export const VisualiserNeighborhood: React.FC<Props> = ({ index, focusId, onFocus }) => {
    const [nodes, setNodes] = useState<DisplayNode[]>([]);
    const [edges, setEdges] = useState<DisplayEdge[]>([]);
    const [activeFocusId, setActiveFocusId] = useState<number | null>(null);
    const [viewH, setViewH] = useState(0);

    useEffect(() => {
        if (focusId === null) return;

        // Initial mount: no transition needed
        if (nodes.length === 0 || activeFocusId === null) {
            const next = buildLayout(focusId, index);
            setNodes(next.nodes.map(n => ({ ...n, state: "visible" })));
            setEdges(next.edges.map(e => ({ ...e, state: "visible" })));
            setViewH(next.viewH);
            setActiveFocusId(focusId);
            return;
        }

        if (focusId === activeFocusId) return;

        const next = buildLayout(focusId, index);
        const nextIds = new Set(next.nodes.map(n => n.id));
        const nextEdgeIds = new Set(next.edges.map(e => e.id));

        // Stage 1: classify current nodes
        const stage1: DisplayNode[] = [];
        const seen = new Set<number>();
        for (const n of nodes) {
            if (nextIds.has(n.id)) {
                // Stayer: move to new position, update kind for new role
                const target = next.nodes.find(t => t.id === n.id)!;
                stage1.push({ ...target, state: "visible" });
                seen.add(n.id);
            } else {
                // Leaver: hold position, fade out
                stage1.push({ ...n, state: "leaving" });
            }
        }
        // Enterers: in new but not in old — mount at target position with opacity 0
        for (const t of next.nodes) {
            if (!seen.has(t.id)) {
                stage1.push({ ...t, state: "entering" });
            }
        }

        // Stage 1: classify current edges
        const stage1Edges: DisplayEdge[] = [];
        const edgeSeen = new Set<string>();
        for (const e of edges) {
            if (nextEdgeIds.has(e.id)) {
                const target = next.edges.find(t => t.id === e.id)!;
                stage1Edges.push({ ...target, state: "visible" });
                edgeSeen.add(e.id);
            } else {
                stage1Edges.push({ ...e, state: "leaving" });
            }
        }
        for (const t of next.edges) {
            if (!edgeSeen.has(t.id)) {
                stage1Edges.push({ ...t, state: "entering" });
            }
        }

        // Update viewH to new layout immediately so new nodes fit
        setViewH(next.viewH);
        setNodes(stage1);
        setEdges(stage1Edges);

        // After TRANSITION_MS: drop leavers and promote enterers to visible
        // Enterers were mounted at opacity 0; by this point the browser has painted
        // them, so changing to visible triggers the CSS transition to opacity 1.
        const t1 = setTimeout(() => {
            setNodes(prev => prev
                .filter(n => n.state !== "leaving")
                .map(n => ({ ...n, state: "visible" })),
            );
            setEdges(prev => prev
                .filter(e => e.state !== "leaving")
                .map(e => ({ ...e, state: "visible" })),
            );
            setActiveFocusId(focusId);
        }, TRANSITION_MS);

        return () => clearTimeout(t1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [focusId, index]);

    if (focusId === null) {
        return <div className="visualiser-graph-empty">Select a tag to see its neighborhood.</div>;
    }
    if (nodes.length === 0) {
        return <div className="visualiser-graph-empty">Tag not found.</div>;
    }

    const focusNode = nodes.find(n => n.kind === "focus");
    const focusedName = focusNode?.label ?? "";

    const renderChip = (n: DisplayNode) => {
        const maxTextWidth = NODE_W - 8;
        const approxCharWidth = 5.5;
        const naturalWidth = n.label.length * approxCharWidth;
        const shrink = naturalWidth > maxTextWidth;
        return (
            <g
                key={n.id}
                className={classNames("visualiser-graph-node", n.kind, {
                    "is-leaving": n.state === "leaving",
                    "is-entering": n.state === "entering",
                })}
                transform={`translate(${n.x - NODE_W / 2} ${n.y - NODE_H / 2})`}
            >
                <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                      fill={n.colour ?? "var(--primary)"} />
                <text x={NODE_W / 2} y={NODE_H / 2 + 3} textAnchor="middle"
                      textLength={shrink ? maxTextWidth : undefined}
                      lengthAdjust={shrink ? "spacingAndGlyphs" : undefined}>
                    {n.label}
                </text>
                <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                      fill="transparent" role="button" tabIndex={0}
                      aria-label={`${n.kind === "focus" ? "Focused tag" : n.kind === "parent" ? "Parent tag" : "Child tag"}: ${n.label}`}
                      onClick={() => onFocus(n.id)}
                      onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onFocus(n.id); } }}>
                    <title>{n.label}</title>
                </rect>
            </g>
        );
    };

    return (
        <svg className="visualiser-graph" viewBox={`0 0 ${VIEW_W} ${viewH}`} preserveAspectRatio="xMidYMid meet" role="img" aria-label={`Neighborhood of ${focusedName}`}>
            <g className="visualiser-graph-edges">
                {edges.map((e) => (
                    <line
                        key={e.id}
                        className={classNames({ "is-leaving": e.state === "leaving", "is-entering": e.state === "entering" })}
                        x1={e.x1} y1={e.y1} x2={e.x2} y2={e.y2}
                    />
                ))}
            </g>
            {nodes.map(n => renderChip(n))}
        </svg>
    );
};
