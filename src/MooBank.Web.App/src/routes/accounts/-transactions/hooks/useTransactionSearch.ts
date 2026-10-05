import { useMemo } from "react";
import { useDebounce } from "use-debounce";

import { useGridPageSize, useGridSearch } from "hooks/useGridSearch";
import {
    defaultSortDirection,
    defaultSortField,
    defaultTransactionsPageSize,
    resolveTransactionSearch,
    searchToFilter,
    transactionsPageSizeKey,
    type TransactionSearch,
} from "../transactionSearch";

// Single source of truth for the transaction-list UI state. Filter/sort/page/page size come from
// the route search params; without a pageSize param, the persisted preference applies.
export const useTransactionSearch = () => {

    const { search, page, sortField, sortDirection, setPage, setSort, setFilter } = useGridSearch<TransactionSearch>({ sortField: defaultSortField, sortDirection: defaultSortDirection });
    const [pageSize, setPageSize] = useGridPageSize(transactionsPageSizeKey, defaultTransactionsPageSize);

    // Resolve defaults (persisted filters + default period) so the filter carries a date range from
    // the first render — the query is then enabled immediately (and hits the loader-warmed cache)
    // instead of waiting for the filter panel's post-mount effect to write params to the URL. `search`
    // stays raw for the panel, which distinguishes explicit URL/widget params from these defaults.
    // Memoise so the debounced filter has a stable reference to track (searchToFilter builds a
    // fresh object each render); debounce the whole filter at the query.
    const filter = useMemo(() => searchToFilter(resolveTransactionSearch(search as Record<string, unknown>)), [search]);
    const [debouncedFilter] = useDebounce(filter, 250);

    return { search, filter, debouncedFilter, page, pageSize, setPageSize, sortField, sortDirection, setPage, setSort, setFilter };
};
