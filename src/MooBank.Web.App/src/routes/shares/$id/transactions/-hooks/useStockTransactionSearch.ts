import { useMemo } from "react";
import { useNavigate, useSearch } from "@tanstack/react-router";
import { useDebounce } from "use-debounce";
import type { SortDirection } from "@andrewmclachlan/moo-ds";

import {
    defaultStockSortDirection,
    defaultStockSortField,
    stockSearchToFilter,
    type StockTransactionSearch,
} from "../stockTransactionSearch";

// Fixed page size: the stock-transaction list has no page-size control (the former slice hard-coded 50).
const stockPageSize = 50;

// Single source of truth for the stock-transaction-list UI state (formerly the StockTransactions
// Redux slice). Filter/sort/page live in the route search params.
export const useStockTransactionSearch = () => {

    const search = useSearch({ strict: false }) as StockTransactionSearch;
    const navigate = useNavigate();

    const page = search.page ?? 1;
    const sortField = search.sortField ?? defaultStockSortField;
    const sortDirection = search.sortDirection ?? defaultStockSortDirection;
    // Memoise so the debounced filter has a stable reference; debounce the whole filter at the
    // query, as the former slice did.
    const filter = useMemo(() => stockSearchToFilter(search), [search]);
    const [debouncedFilter] = useDebounce(filter, 250);

    const patch = (values: Partial<StockTransactionSearch>) =>
        navigate({ search: ((prev: StockTransactionSearch) => ({ ...prev, ...values })) as any, replace: true });

    const setPage = (newPage: number) => patch({ page: newPage > 1 ? newPage : undefined });

    const setSort = (field: string, direction: SortDirection) => patch({ sortField: field, sortDirection: direction });

    // Applying a filter always returns to the first page.
    const setFilter = (values: Partial<StockTransactionSearch>) => patch({ ...values, page: undefined });

    return { search, filter, debouncedFilter, page, pageSize: stockPageSize, sortField, sortDirection, setPage, setSort, setFilter };
};
