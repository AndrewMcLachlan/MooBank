import { useSelector } from "react-redux";
import { useDebounce } from "use-debounce";
import { parseISO } from "date-fns";

import { useInOutReport } from "hooks/useInOutReport";
import { useTransactions } from "routes/accounts/-hooks/useTransactions";
import type { State } from "store/state";

export interface PeriodStats {
    income: number;
    expenses: number;
    net: number;
    total: number;
}

export const useTransactionPeriodStats = (accountId: string): PeriodStats => {
    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);
    const [debouncedFilter] = useDebounce(filter, 250);

    const hasPeriod = !!debouncedFilter.start && !!debouncedFilter.end;
    const start = hasPeriod ? parseISO(debouncedFilter.start!) : new Date(0);
    const end = hasPeriod ? parseISO(debouncedFilter.end!) : new Date(0);
    const { data: report } = useInOutReport(hasPeriod ? accountId : "", start, end);

    const { data } = useTransactions(accountId, debouncedFilter, pageSize, currentPage, sortField, sortDirection);

    const income = report?.income ?? 0;
    const expenses = report?.outgoings ?? 0;

    return {
        income,
        expenses,
        net: income + expenses,
        total: data?.total ?? 0,
    };
};
