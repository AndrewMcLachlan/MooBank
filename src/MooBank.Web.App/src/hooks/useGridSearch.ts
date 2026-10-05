import { useEffect, useRef, useState } from "react";
import { useNavigate, useSearch } from "@tanstack/react-router";
import { useLocalStorage } from "@andrewmclachlan/moo-ds";
import type { SortDirection } from "@andrewmclachlan/moo-ds";
import { useDebouncedCallback } from "use-debounce";

import { clampPage, isSearchChange, parsePageSize, type GridSearch, type GridSearchDefaults } from "utils/gridSearch";

const useUpdateSearch = <T extends object>() => {
    const navigate = useNavigate();
    return (values: Partial<T>, replace = false) =>
        navigate({ search: ((prev: T) => ({ ...prev, ...values })) as any, replace });
};

/**
 * Paging, sorting and filtering state for a data grid, held in the route's search params so that
 * refresh, back/forward and shared links keep it. Page and sort changes add a history entry;
 * filter changes replace the current one and return to the first page.
 */
export const useGridSearch = <T extends GridSearch = GridSearch>({ sortField: defaultSortField, sortDirection: defaultSortDirection = "Ascending" }: GridSearchDefaults = {}) => {

    const search = useSearch({ strict: false }) as T;
    const update = useUpdateSearch<T>();

    const page = search.page ?? 1;
    const sortField = search.sortField ?? defaultSortField;
    const sortDirection = search.sortDirection ?? defaultSortDirection;

    const setPage = (newPage: number) => update({ page: newPage > 1 ? newPage : undefined } as Partial<T>);

    const resetPage = () => {
        if (search.page) update({ page: undefined } as Partial<T>, true);
    };

    const setSort = (field: string, direction: SortDirection) => update({
        sortField: field === defaultSortField ? undefined : field,
        sortDirection: direction === defaultSortDirection ? undefined : direction,
    } as Partial<T>);

    const setFilter = (values: Partial<T>) => {
        if (!isSearchChange(search, values)) return;
        update({ ...values, page: undefined }, true);
    };

    return { search, page, sortField, sortDirection, setPage, resetPage, setSort, setFilter };
};

/**
 * The grid's page size: the search param when the link carries one, otherwise the user's stored
 * preference. Choosing a size updates both.
 */
export const useGridPageSize = (storageKey: string, defaultPageSize: number) => {

    const search = useSearch({ strict: false }) as GridSearch;
    const update = useUpdateSearch<GridSearch>();
    const [storedPageSize, setStoredPageSize] = useLocalStorage<number>(storageKey, defaultPageSize);

    const pageSize = search.pageSize ?? parsePageSize(storedPageSize) ?? defaultPageSize;

    const setPageSize = (newPageSize: number) => {
        setStoredPageSize(newPageSize);
        update({ pageSize: newPageSize === defaultPageSize ? undefined : newPageSize });
    };

    return [pageSize, setPageSize] as const;
};

/**
 * Moves a page number beyond the last page (a stale link, or rows removed) back onto the last
 * page. `ready` must stay false until `numberOfPages` reflects loaded data.
 */
export const usePageInRange = (page: number, numberOfPages: number, ready: boolean) => {

    const update = useUpdateSearch<GridSearch>();
    const inRange = clampPage(page, numberOfPages);

    useEffect(() => {
        if (!ready || inRange === page) return;
        update({ page: inRange > 1 ? inRange : undefined }, true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ready, page, inRange]);
};

/**
 * Input state for a grid's search box. Keystrokes update the box immediately and reach the
 * search params once typing pauses; a change to the params from elsewhere (back/forward) is
 * reflected in the box.
 */
export const useSearchTerm = (value: string, onCommit: (term: string) => void, delay = 250) => {

    const [term, setTerm] = useState(value);
    const committedRef = useRef(value);

    useEffect(() => {
        if (value === committedRef.current) return;
        committedRef.current = value;
        setTerm(value);
    }, [value]);

    const commit = useDebouncedCallback((newTerm: string) => {
        committedRef.current = newTerm;
        onCommit(newTerm);
    }, delay);

    const changeTerm = (newTerm: string) => {
        setTerm(newTerm);
        commit(newTerm);
    };

    return [term, changeTerm] as const;
};
