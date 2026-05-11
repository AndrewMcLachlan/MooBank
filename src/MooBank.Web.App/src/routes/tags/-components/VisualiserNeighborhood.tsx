import classNames from "classnames";
import { useEffect, useState } from "react";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";

const FADE_MS = 140;

interface Props {
    index: TagsGraphIndex;
    focusId: number | null;
    onFocus: (id: number) => void;
}

interface Positioned {
    id: number;
    x: number;
    y: number;
    label: string;
    colour?: string | null;
}

const VIEW_W = 600;
const NODE_W = 84;
const NODE_H = 22;
const COL_GAP = 8;
const ROW_GAP = 8;
const PAD_X = 12;
const FOCUS_GAP_Y = 28;     // vertical gap between focus and the nearest parent/child row
const OUTER_PAD_Y = 12;

const chipsPerRow = (): number => {
    const usable = VIEW_W - PAD_X * 2;
    const slot = NODE_W + COL_GAP;
    return Math.max(1, Math.floor((usable + COL_GAP) / slot));
};

interface RowLayout {
    rows: Positioned[][];
    heightAboveAnchor: number;
    heightBelowAnchor: number;
}

const layoutGroup = (
    ids: number[],
    anchorY: number,
    direction: -1 | 1,
    index: TagsGraphIndex,
): RowLayout => {
    if (ids.length === 0) {
        return { rows: [], heightAboveAnchor: 0, heightBelowAnchor: 0 };
    }

    const perRow = chipsPerRow();
    const numRows = Math.ceil(ids.length / perRow);

    // For parents (direction=-1), the row closest to focus is the FIRST row (rowIndex 0).
    // For children (direction=+1), same: rowIndex 0 is closest to focus.
    const rows: Positioned[][] = [];
    for (let r = 0; r < numRows; r++) {
        const start = r * perRow;
        const end = Math.min(start + perRow, ids.length);
        const inThisRow = end - start;

        const totalWidth = inThisRow * NODE_W + (inThisRow - 1) * COL_GAP;
        const rowStartX = (VIEW_W - totalWidth) / 2;

        // y of row r: anchorY + direction * (FOCUS_GAP_Y + r*(NODE_H+ROW_GAP) + NODE_H/2)
        const y = anchorY + direction * (FOCUS_GAP_Y + r * (NODE_H + ROW_GAP) + NODE_H / 2);

        const row: Positioned[] = [];
        for (let i = 0; i < inThisRow; i++) {
            const id = ids[start + i];
            const node = index.byId.get(id);
            if (!node) continue;
            const x = rowStartX + i * (NODE_W + COL_GAP) + NODE_W / 2;
            row.push({ id, x, y, label: node.name, colour: node.colour as string | null });
        }
        rows.push(row);
    }

    const groupHeight = FOCUS_GAP_Y + numRows * NODE_H + (numRows - 1) * ROW_GAP;
    return {
        rows,
        heightAboveAnchor: direction === -1 ? groupHeight : 0,
        heightBelowAnchor: direction === 1 ? groupHeight : 0,
    };
};

export const VisualiserNeighborhood: React.FC<Props> = ({ index, focusId, onFocus }) => {
    const [displayId, setDisplayId] = useState<number | null>(focusId);
    const [fading, setFading] = useState(false);

    useEffect(() => {
        if (focusId === displayId) return;
        setFading(true);
        const t = setTimeout(() => {
            setDisplayId(focusId);
            setFading(false);
        }, FADE_MS);
        return () => clearTimeout(t);
    }, [focusId, displayId]);

    if (displayId === null) {
        return <div className="visualiser-graph-empty">Select a tag to see its neighborhood.</div>;
    }
    const focused = index.byId.get(displayId);
    if (!focused) {
        return <div className="visualiser-graph-empty">Tag not found.</div>;
    }

    const parentIds = index.parentsOf.get(displayId) ?? [];
    const childIds = index.childrenOf.get(displayId) ?? [];

    // First pass: figure out total height so we can position the focus chip
    const parentRows = Math.max(1, Math.ceil(parentIds.length / chipsPerRow()));
    const childRows = Math.max(1, Math.ceil(childIds.length / chipsPerRow()));
    const parentHeight = parentIds.length === 0
        ? 0
        : FOCUS_GAP_Y + parentRows * NODE_H + (parentRows - 1) * ROW_GAP;
    const childHeight = childIds.length === 0
        ? 0
        : FOCUS_GAP_Y + childRows * NODE_H + (childRows - 1) * ROW_GAP;

    const focusY = OUTER_PAD_Y + parentHeight + NODE_H / 2;
    const viewH = OUTER_PAD_Y + parentHeight + NODE_H + childHeight + OUTER_PAD_Y;

    const parents = layoutGroup(parentIds, focusY, -1, index);
    const children = layoutGroup(childIds, focusY, 1, index);
    const focus: Positioned = {
        id: displayId,
        x: VIEW_W / 2,
        y: focusY,
        label: focused.name,
        colour: focused.colour as string | null,
    };

    const renderChip = (p: Positioned, kind: "parent" | "focus" | "child") => {
        const maxTextWidth = NODE_W - 8;
        const approxCharWidth = 5.5;
        const naturalWidth = p.label.length * approxCharWidth;
        const shrink = naturalWidth > maxTextWidth;
        return (
            <g key={`${kind}-${p.id}`} className={classNames("visualiser-graph-node", kind)}
               transform={`translate(${p.x - NODE_W / 2} ${p.y - NODE_H / 2})`}>
                <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                      fill={p.colour ?? "var(--primary)"} />
                <text x={NODE_W / 2} y={NODE_H / 2 + 3} textAnchor="middle"
                      textLength={shrink ? maxTextWidth : undefined}
                      lengthAdjust={shrink ? "spacingAndGlyphs" : undefined}>
                    {p.label}
                </text>
                <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                      fill="transparent" role="button" tabIndex={0}
                      aria-label={`${kind === "focus" ? "Focused tag" : kind === "parent" ? "Parent tag" : "Child tag"}: ${p.label}`}
                      onClick={() => onFocus(p.id)}
                      onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onFocus(p.id); } }}>
                    <title>{p.label}</title>
                </rect>
            </g>
        );
    };

    return (
        <svg className={classNames("visualiser-graph", { "is-fading": fading })} viewBox={`0 0 ${VIEW_W} ${viewH}`} preserveAspectRatio="xMidYMid meet" role="img" aria-label={`Neighborhood of ${focused.name}`}>
            <g className="visualiser-graph-edges">
                {parents.rows.flat().map((p) => (
                    <line key={`pe-${p.id}`} x1={p.x} y1={p.y + NODE_H / 2} x2={focus.x} y2={focus.y - NODE_H / 2} />
                ))}
                {children.rows.flat().map((c) => (
                    <line key={`ce-${c.id}`} x1={focus.x} y1={focus.y + NODE_H / 2} x2={c.x} y2={c.y - NODE_H / 2} />
                ))}
            </g>
            {parents.rows.flat().map((p) => renderChip(p, "parent"))}
            {renderChip(focus, "focus")}
            {children.rows.flat().map((c) => renderChip(c, "child"))}
        </svg>
    );
};
