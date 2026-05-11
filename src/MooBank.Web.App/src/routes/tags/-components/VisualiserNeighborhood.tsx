import classNames from "classnames";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";

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
const VIEW_H = 360;
const ROW_PARENT_Y = 50;
const ROW_FOCUS_Y = 180;
const ROW_CHILD_Y = 310;
const NODE_W = 110;
const NODE_H = 30;

const layoutRow = (ids: number[], y: number, index: TagsGraphIndex): Positioned[] => {
    if (ids.length === 0) return [];
    const padding = NODE_W / 2;
    const usable = VIEW_W - padding * 2;
    const step = ids.length === 1 ? 0 : usable / (ids.length - 1);
    return ids.map((id, i) => {
        const node = index.byId.get(id)!;
        const x = ids.length === 1 ? VIEW_W / 2 : padding + step * i;
        return { id, x, y, label: node.name, colour: node.colour as string | null };
    });
};

export const VisualiserNeighborhood: React.FC<Props> = ({ index, focusId, onFocus }) => {
    if (focusId === null) {
        return <div className="visualiser-graph-empty">Select a tag to see its neighborhood.</div>;
    }
    const focused = index.byId.get(focusId);
    if (!focused) {
        return <div className="visualiser-graph-empty">Tag not found.</div>;
    }

    const parentIds = index.parentsOf.get(focusId) ?? [];
    const childIds = index.childrenOf.get(focusId) ?? [];

    const parents = layoutRow(parentIds, ROW_PARENT_Y, index);
    const children = layoutRow(childIds, ROW_CHILD_Y, index);
    const focus: Positioned = { id: focusId, x: VIEW_W / 2, y: ROW_FOCUS_Y, label: focused.name, colour: focused.colour as string | null };

    const renderChip = (p: Positioned, kind: "parent" | "focus" | "child") => (
        <g key={`${kind}-${p.id}`} className={classNames("visualiser-graph-node", kind)}
           transform={`translate(${p.x - NODE_W / 2} ${p.y - NODE_H / 2})`}>
            <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                  fill={p.colour ?? "var(--visualiser-chip-fallback, #3b5b88)"} />
            <text x={NODE_W / 2} y={NODE_H / 2 + 4} textAnchor="middle">{p.label}</text>
            <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                  fill="transparent" role="button" tabIndex={0}
                  aria-label={`${kind === "focus" ? "Focused tag" : kind === "parent" ? "Parent tag" : "Child tag"}: ${p.label}`}
                  onClick={() => onFocus(p.id)}
                  onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onFocus(p.id); } }} />
        </g>
    );

    return (
        <svg className="visualiser-graph" viewBox={`0 0 ${VIEW_W} ${VIEW_H}`} preserveAspectRatio="xMidYMid meet" role="img" aria-label={`Neighborhood of ${focused.name}`}>
            <g className="visualiser-graph-edges">
                {parents.map((p) => (
                    <line key={`pe-${p.id}`} x1={p.x} y1={p.y + NODE_H / 2} x2={focus.x} y2={focus.y - NODE_H / 2} />
                ))}
                {children.map((c) => (
                    <line key={`ce-${c.id}`} x1={focus.x} y1={focus.y + NODE_H / 2} x2={c.x} y2={c.y - NODE_H / 2} />
                ))}
            </g>
            {parents.map((p) => renderChip(p, "parent"))}
            {renderChip(focus, "focus")}
            {children.map((c) => renderChip(c, "child"))}
        </svg>
    );
};
