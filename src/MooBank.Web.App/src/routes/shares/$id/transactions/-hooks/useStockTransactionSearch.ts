import { useMemo } from "react";
import { useDebounce } from "use-debounce";

import { useGridSearch } from "hooks/useGridSearch";
import {
    defaultStockSortDirection,
    defaultStockSortField,
    stockSearchToFilter,
    type StockTransactionSearch,
} from "../-stockTransactionSearch";

// Fixed page size: the stock-transaction list has no page-size control.
const stockPageSize = 50;

// Single source of truth for the stock-transaction-list UI state. Filter/sort/page live in the
// route search params.
export const useStockTransactionSearch = () => {

    const { search, page, sortField, sortDirection, setPage, setSort, setFilter } = useGridSearch<StockTransactionSearch>({
        sortField: defaultStockSortField,
        sortDirection: defaultStockSortDirection,
    });

    // Memoise so the debounced filter has a stable reference; debounce the whole filter at the query.
    const filter = useMemo(() => stockSearchToFilter(search), [search]);
    const [debouncedFilter] = useDebounce(filter, 250);

    return { search, filter, debouncedFilter, page, pageSize: stockPageSize, sortField, sortDirection, setPage, setSort, setFilter };
};
