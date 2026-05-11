import classNames from "classnames";
import type { TagGraphNode } from "api/types.gen";

interface Props {
    node: TagGraphNode;
    pathKey: string;
    level: number;
    isFocused: boolean;
    isExpanded: boolean;
    hasChildren: boolean;
    isCycle: boolean;
    searchTerm: string;
    onToggle: (pathKey: string) => void;
    onFocus: (id: number) => void;
}

const highlightMatch = (text: string, term: string): React.ReactNode => {
    if (!term) return text;
    const lower = text.toLowerCase();
    const t = term.toLowerCase();
    const i = lower.indexOf(t);
    if (i < 0) return text;
    return (
        <>
            {text.slice(0, i)}
            <mark>{text.slice(i, i + t.length)}</mark>
            {text.slice(i + t.length)}
        </>
    );
};

export const VisualiserOutlineRow: React.FC<Props> = ({
    node, pathKey, level, isFocused, isExpanded, hasChildren, isCycle, searchTerm, onToggle, onFocus,
}) => {
    const swatchStyle = node.colour ? { backgroundColor: String(node.colour) } : undefined;

    return (
        <div
            role="treeitem"
            aria-level={level + 1}
            aria-expanded={hasChildren ? isExpanded : undefined}
            aria-selected={isFocused}
            className={classNames("visualiser-row", { focused: isFocused })}
            style={{ paddingLeft: `${level * 18 + 8}px` }}
        >
            <button
                type="button"
                className="visualiser-twirl"
                aria-label={isExpanded ? "Collapse" : "Expand"}
                tabIndex={-1}
                disabled={!hasChildren || isCycle}
                onClick={(e) => { e.stopPropagation(); onToggle(pathKey); }}
            >
                {hasChildren && !isCycle ? (isExpanded ? "▼" : "▶") : ""}
            </button>
            <button
                type="button"
                className="visualiser-chip"
                onClick={() => onFocus(node.id)}
            >
                <span className="visualiser-chip-swatch" style={swatchStyle} aria-hidden="true" />
                <span className="visualiser-chip-name">{highlightMatch(node.name, searchTerm)}</span>
                {isCycle && <span className="visualiser-cycle" title="Cycle in tag graph — not expanded">↺</span>}
            </button>
        </div>
    );
};
