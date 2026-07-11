import { getPeriodLabel } from "utils/periodLabel";
import { useStockTransactionSearch } from "./useStockTransactionSearch";

export const usePeriodLabel = (): string => {
    const { filter } = useStockTransactionSearch();
    return getPeriodLabel(filter);
};
