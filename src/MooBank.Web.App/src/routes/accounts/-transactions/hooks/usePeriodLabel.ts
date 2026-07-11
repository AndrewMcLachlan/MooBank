import { getPeriodLabel } from "utils/periodLabel";
import { useTransactionSearch } from "./useTransactionSearch";

export const usePeriodLabel = (): string => {
    const { filter } = useTransactionSearch();
    return getPeriodLabel(filter);
};
