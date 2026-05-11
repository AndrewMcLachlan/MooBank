import { useCallback, useEffect, useMemo, useState } from "react";

const STORAGE_KEY = "moobank.visualiser.state.v1";

interface PersistedState {
    expanded: string[];
    focusId: number | null;
}

const loadPersisted = (): PersistedState => {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return { expanded: [], focusId: null };
        const parsed = JSON.parse(raw) as Partial<PersistedState>;
        return {
            expanded: Array.isArray(parsed.expanded) ? parsed.expanded : [],
            focusId: typeof parsed.focusId === "number" ? parsed.focusId : null,
        };
    } catch {
        return { expanded: [], focusId: null };
    }
};

const persist = (state: PersistedState): void => {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
        // ignore quota / privacy-mode errors
    }
};

export interface VisualiserState {
    expanded: Set<string>;
    isExpanded: (pathKey: string) => boolean;
    toggleExpand: (pathKey: string) => void;

    focusId: number | null;
    setFocusId: (id: number | null) => void;

    search: string;
    setSearch: (s: string) => void;
}

export interface UseVisualiserStateOptions {
    /** Path keys to expand on first ever visit (used when localStorage is empty). */
    initialExpanded?: string[];
}

export const useVisualiserState = (options: UseVisualiserStateOptions = {}): VisualiserState => {
    const [expanded, setExpanded] = useState<Set<string>>(() => {
        const persisted = loadPersisted();
        if (persisted.expanded.length > 0) return new Set(persisted.expanded);
        return new Set(options.initialExpanded ?? []);
    });

    const [focusId, setFocusIdState] = useState<number | null>(() => loadPersisted().focusId);
    const [search, setSearch] = useState<string>("");

    useEffect(() => {
        persist({ expanded: Array.from(expanded), focusId });
    }, [expanded, focusId]);

    const isExpanded = useCallback((pathKey: string) => expanded.has(pathKey), [expanded]);

    const toggleExpand = useCallback((pathKey: string) => {
        setExpanded((prev) => {
            const next = new Set(prev);
            if (next.has(pathKey)) next.delete(pathKey);
            else next.add(pathKey);
            return next;
        });
    }, []);

    const setFocusId = useCallback((id: number | null) => setFocusIdState(id), []);

    return useMemo<VisualiserState>(() => ({
        expanded, isExpanded, toggleExpand,
        focusId, setFocusId,
        search, setSearch,
    }), [expanded, isExpanded, toggleExpand, focusId, setFocusId, search]);
};
