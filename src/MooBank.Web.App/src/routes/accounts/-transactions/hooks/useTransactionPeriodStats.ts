import { useMemo } from "react";
import { useSelector } from "react-redux";
import { useDebounce } from "use-debounce";

import type { Transaction } from "api/types.gen";
import { useTransactionList } from "components";
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
    const { showNet } = useTransactionList();
    const { data } = useTransactions(accountId, debouncedFilter, pageSize, currentPage, sortField, sortDirection);

    const stats = useMemo(() => calculate(data?.results, showNet), [data?.results, showNet]);
    return { ...stats, total: data?.total ?? 0 };
};

const calculate = (transactions: Transaction[] | undefined, showNet: boolean) => {
    if (!transactions || transactions.length === 0) {
        return { income: 0, expenses: 0, net: 0 };
    }
    let income = 0;
    let expenses = 0;
    for (const t of transactions) {
        const a = showNet ? t.netAmount : t.amount;
        if (a > 0) income += a;
        else if (a < 0) expenses += a;
    }
    return { income, expenses: Math.abs(expenses), net: income + expenses };
};
