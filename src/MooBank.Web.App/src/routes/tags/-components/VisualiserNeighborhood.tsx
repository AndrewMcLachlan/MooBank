import classNames from "classnames";
import { useEffect, useState } from "react";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";

const TRANSITION_MS = 400;

interface Props {
    index: TagsGraphIndex;
    focusId: number | null;
    onFocus: (id: number) => void;
}

const VIEW_W = 800;
const VIEW_H = 600;
const NODE_W = 84;
const NODE_H = 22;
const COL_GAP = 8;
const ROW_GAP = 8;
const PAD_X = 12;
const FOCUS_GAP_Y = 28;
// Focus position is fixed in viewBox coordinates so chips render at a
// stable screen size regardless of how many parents/children exist.
// 40% from the top leaves more vertical space below for children
// (typically more numerous than parents).
const FOCUS_Y = VIEW_H * 0.40;

type NodeState = "visible" | "leaving" | "entering";

interface DisplayNode {
    id: number;
    x: number;
    y: number;
    label: string;
    colour: string | null;
    kind: "parent" | "focus" | "child";
    childCount: number;
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
}

const chipsPerRow = (): number => {
    const usable = VIEW_W - PAD_X * 2;
    const slot = NODE_W + COL_GAP;
    return Math.max(1, Math.floor((usable + COL_GAP) / slot));
};

const buildLayout = (forFocusId: number, index: TagsGraphIndex): Layout => {
    const focused = index.byId.get(forFocusId);
    if (!focused) return { nodes: [], edges: [] };

    const parentIds = index.parentsOf.get(forFocusId) ?? [];
    const childIds = index.childrenOf.get(forFocusId) ?? [];

    const perRow = chipsPerRow();
    const focusY = FOCUS_Y;

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
        childCount: index.childrenOf.get(forFocusId)?.length ?? 0,
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
                nodes.push({
                    id, x, y,
                    label: node.name,
                    colour: node.colour as string | null,
                    kind,
                    childCount: index.childrenOf.get(id)?.length ?? 0,
                });
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

    return { nodes, edges };
};

export const VisualiserNeighborhood: React.FC<Props> = ({ index, focusId, onFocus }) => {
    const [nodes, setNodes] = useState<DisplayNode[]>([]);
    const [edges, setEdges] = useState<DisplayEdge[]>([]);
    const [activeFocusId, setActiveFocusId] = useState<number | null>(null);

    useEffect(() => {
        if (focusId === null) return;

        // Initial mount: no transition needed
        if (nodes.length === 0 || activeFocusId === null) {
            const next = buildLayout(focusId, index);
            setNodes(next.nodes.map(n => ({ ...n, state: "visible" })));
            setEdges(next.edges.map(e => ({ ...e, state: "visible" })));
            setActiveFocusId(focusId);
            return;
        }

        if (focusId === activeFocusId) return;

        const next = buildLayout(focusId, index);
        const nextIds = new Set(next.nodes.map(n => n.id));
        const nextEdgeIds = new Set(next.edges.map(e => e.id));

        // STAGE 1 (t=0 → TRANSITION_MS): old layout transitioning to new.
        //   - Stayers: switch to new position (CSS transitions transform/d smoothly).
        //   - Leavers: hold old position, fade opacity → 0.
        //   - Enterers: NOT in the DOM yet, so the user only sees the stayers
        //     moving and the leavers fading out — no premature lines/chips
        //     at the destination.
        const stage1Nodes: DisplayNode[] = [];
        const seenNodes = new Set<number>();
        for (const n of nodes) {
            if (nextIds.has(n.id)) {
                const target = next.nodes.find(t => t.id === n.id)!;
                stage1Nodes.push({ ...target, state: "visible" });
                seenNodes.add(n.id);
            } else {
                stage1Nodes.push({ ...n, state: "leaving" });
            }
        }

        const stage1Edges: DisplayEdge[] = [];
        const seenEdges = new Set<string>();
        for (const e of edges) {
            if (nextEdgeIds.has(e.id)) {
                const target = next.edges.find(t => t.id === e.id)!;
                stage1Edges.push({ ...target, state: "visible" });
                seenEdges.add(e.id);
            } else {
                stage1Edges.push({ ...e, state: "leaving" });
            }
        }

        setNodes(stage1Nodes);
        setEdges(stage1Edges);

        let t2: ReturnType<typeof setTimeout> | null = null;

        // STAGE 2 (t=TRANSITION_MS): movement complete. Drop leavers, mount
        // enterers with opacity 0 (CSS hides them via .is-entering).
        const t1 = setTimeout(() => {
            const stage2Nodes: DisplayNode[] = stage1Nodes
                .filter(n => n.state !== "leaving")
                .map(n => ({ ...n, state: "visible" as const }));
            for (const t of next.nodes) {
                if (!seenNodes.has(t.id)) {
                    stage2Nodes.push({ ...t, state: "entering" });
                }
            }
            const stage2Edges: DisplayEdge[] = stage1Edges
                .filter(e => e.state !== "leaving")
                .map(e => ({ ...e, state: "visible" as const }));
            for (const t of next.edges) {
                if (!seenEdges.has(t.id)) {
                    stage2Edges.push({ ...t, state: "entering" });
                }
            }
            setNodes(stage2Nodes);
            setEdges(stage2Edges);

            // STAGE 3 (one frame later): flip enterers to visible. The browser
            // has painted them at opacity 0, so this triggers the fade-in.
            t2 = setTimeout(() => {
                setNodes(prev => prev.map(n => ({ ...n, state: "visible" as const })));
                setEdges(prev => prev.map(e => ({ ...e, state: "visible" as const })));
                setActiveFocusId(focusId);
            }, 20);
        }, TRANSITION_MS);

        return () => {
            clearTimeout(t1);
            if (t2) clearTimeout(t2);
        };
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
                {n.childCount > 0 && (
                    <g className="visualiser-graph-childcount" transform={`translate(${NODE_W - 3} -3)`}>
                        <circle r={7} />
                        <text textAnchor="middle" y={3}>{n.childCount}</text>
                    </g>
                )}
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
        <svg className="visualiser-graph" viewBox={`0 0 ${VIEW_W} ${VIEW_H}`} preserveAspectRatio="xMidYMid meet" role="img" aria-label={`Neighborhood of ${focusedName}`}>
            <g className="visualiser-graph-edges">
                {edges.map((e) => (
                    <path
                        key={e.id}
                        className={classNames({ "is-leaving": e.state === "leaving", "is-entering": e.state === "entering" })}
                        d={`M ${e.x1} ${e.y1} L ${e.x2} ${e.y2}`}
                    />
                ))}
            </g>
            {nodes.map(n => renderChip(n))}
        </svg>
    );
};
