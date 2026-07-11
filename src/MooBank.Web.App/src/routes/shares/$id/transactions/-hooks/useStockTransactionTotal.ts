import { useStockTransactions } from "routes/shares/-hooks/useStockTransactions";
import { useStockTransactionSearch } from "./useStockTransactionSearch";

export const useStockTransactionTotal = (holdingId: string): number => {
    const { filter, page, pageSize, sortField, sortDirection } = useStockTransactionSearch();
    const { data } = useStockTransactions(holdingId, filter, pageSize, page, sortField, sortDirection);
    return data?.total ?? 0;
};
