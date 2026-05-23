import { useSelector } from "react-redux";
import { useDebounce } from "use-debounce";

import { useStockTransactions } from "routes/shares/-hooks/useStockTransactions";
import type { State } from "store/state";

export const useStockTransactionTotal = (holdingId: string): number => {
    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((s: State) => s.stockTransactions);
    const [debouncedFilter] = useDebounce(filter, 250);
    const { data } = useStockTransactions(holdingId, debouncedFilter, pageSize, currentPage, sortField, sortDirection);
    return data?.total ?? 0;
};
