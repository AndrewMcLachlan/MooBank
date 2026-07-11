import { useNavigate, useSearch } from "@tanstack/react-router";
import { useLocalStorage } from "@andrewmclachlan/moo-ds";
import type { SortDirection } from "@andrewmclachlan/moo-ds";

import {
    defaultSortDirection,
    defaultSortField,
    searchToFilter,
    type TransactionSearch,
} from "../transactionSearch";

// Single source of truth for the transaction-list UI state (formerly the Redux slice).
// Filter/sort/page come from the route search params; pageSize is a persisted preference.
export const useTransactionSearch = () => {

    const search = useSearch({ strict: false }) as TransactionSearch;
    const navigate = useNavigate();
    const [pageSize, setPageSize] = useLocalStorage<number>("transactions-page-size", 50);

    const page = search.page ?? 1;
    const sortField = search.sortField ?? defaultSortField;
    const sortDirection = search.sortDirection ?? defaultSortDirection;
    const filter = searchToFilter(search);

    const patch = (values: Partial<TransactionSearch>) =>
        navigate({ search: ((prev: TransactionSearch) => ({ ...prev, ...values })) as any, replace: true });

    const setPage = (newPage: number) => patch({ page: newPage > 1 ? newPage : undefined });

    const setSort = (field: string, direction: SortDirection) => patch({ sortField: field, sortDirection: direction });

    // Applying a filter always returns to the first page.
    const setFilter = (values: Partial<TransactionSearch>) => patch({ ...values, page: undefined });

    return { search, filter, page, pageSize, setPageSize, sortField, sortDirection, setPage, setSort, setFilter };
};
