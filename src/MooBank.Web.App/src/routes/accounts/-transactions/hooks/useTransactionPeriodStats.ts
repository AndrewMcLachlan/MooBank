import { parseISO } from "date-fns";

import { useInOutReport } from "hooks/useInOutReport";
import { useTransactions } from "routes/accounts/-hooks/useTransactions";
import { useTransactionSearch } from "./useTransactionSearch";

export interface PeriodStats {
    income: number;
    expenses: number;
    net: number;
    total: number;
}

export const useTransactionPeriodStats = (accountId: string): PeriodStats => {
    const { filter, page, pageSize, sortField, sortDirection } = useTransactionSearch();

    const hasPeriod = !!filter.start && !!filter.end;
    const start = hasPeriod ? parseISO(filter.start!) : new Date(0);
    const end = hasPeriod ? parseISO(filter.end!) : new Date(0);
    const { data: report } = useInOutReport(hasPeriod ? accountId : "", start, end);

    const { data } = useTransactions(accountId, filter, pageSize, page, sortField, sortDirection);

    const income = report?.income ?? 0;
    const expenses = report?.outgoings ?? 0;

    return {
        income,
        expenses,
        net: income + expenses,
        total: data?.total ?? 0,
    };
};
