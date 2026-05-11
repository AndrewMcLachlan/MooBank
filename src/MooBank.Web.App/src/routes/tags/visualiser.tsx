import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useCallback, useEffect, useMemo, useState } from "react";

import { TagsPage } from "./-components/TagsPage";
import { VisualiserNeighborhood } from "./-components/VisualiserNeighborhood";
import { VisualiserOutline } from "./-components/VisualiserOutline";
import { VisualiserSearch } from "./-components/VisualiserSearch";
import { useTagsGraph } from "./-hooks/useTagsGraph";
import { useTagsGraphIndex } from "./-hooks/useTagsGraphIndex";
import { useVisualiserState } from "./-hooks/useVisualiserState";

export interface VisualiserSearchParams {
    focus?: number;
}

export const Route = createFileRoute("/tags/visualiser")({
    validateSearch: (search: Record<string, unknown>): VisualiserSearchParams => {
        const f = search.focus;
        if (f === undefined || f === null || f === "") return {};
        const n = Number(f);
        return Number.isFinite(n) ? { focus: n } : {};
    },
    component: Visualiser,
});

function Visualiser() {
    const { data: graph, isLoading } = useTagsGraph();
    const index = useTagsGraphIndex(graph);
    const { focus: urlFocus } = Route.useSearch();
    const navigate = useNavigate({ from: Route.fullPath });

    const initialExpanded = useMemo(() => {
        if (!index) return [];
        return index.roots.map((rid) => `${rid}`);
    }, [index]);

    const state = useVisualiserState({ initialExpanded });
    const [matchCount, setMatchCount] = useState<number>(0);

    useEffect(() => {
        if (urlFocus !== undefined && urlFocus !== state.focusId) {
            state.setFocusId(urlFocus);
        }
    }, [urlFocus, state]);

    const handleFocus = useCallback((id: number) => {
        state.setFocusId(id);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        navigate({ search: { focus: id } as any, replace: true });
    }, [navigate, state]);

    if (isLoading || !index) {
        return (
            <TagsPage>
                <div className="visualiser-page" aria-busy="true">Loading…</div>
            </TagsPage>
        );
    }

    if (index.byId.size === 0) {
        return (
            <TagsPage>
                <div className="visualiser-page">No tags yet. Create some on the Tags page.</div>
            </TagsPage>
        );
    }

    return (
        <TagsPage>
            <div className="visualiser-page">
                <VisualiserSearch
                    value={state.search}
                    onChange={state.setSearch}
                    matchCount={state.search.length > 0 ? matchCount : undefined}
                />
                <VisualiserOutline
                    index={index}
                    expanded={state.expanded}
                    focusId={state.focusId}
                    search={state.search}
                    onToggle={state.toggleExpand}
                    onFocus={handleFocus}
                    onMatchCountChange={setMatchCount}
                />
                <VisualiserNeighborhood
                    index={index}
                    focusId={state.focusId}
                    onFocus={handleFocus}
                />
            </div>
        </TagsPage>
    );
}
