import React from "react";

import { Amount, Kpi, KpiSub, KpiValue, useAccount } from "components";
import type { LogicalAccount } from "api/types.gen";
import { formatDisplayDate } from "utils/dateFns";

import { usePeriodLabel } from "../hooks/usePeriodLabel";
import { useTransactionPeriodStats } from "../hooks/useTransactionPeriodStats";

export const TransactionsCompactWidgets: React.FC = () => {

    const account = useAccount();
    const stats = useTransactionPeriodStats(account?.id ?? "");
    const periodLabel = usePeriodLabel();

    if (!account) return null;

    const balance = (account as LogicalAccount).currentBalanceLocalCurrency ?? account.currentBalance ?? 0;
    const netTone = stats.net >= 0 ? "income" : "expense";

    return (
        <div className="tx-compact-widgets">
            <Kpi label={`${account.name} · Balance`}>
                <KpiValue className="strong"><Amount amount={balance} currencyCode={account.currency} minus /></KpiValue>
                <KpiSub>Last tx · {formatDisplayDate(account.lastTransaction)}</KpiSub>
            </Kpi>
            <Kpi label="Income" tone="income">
                <KpiValue><Amount amount={stats.income} currencyCode={account.currency} positiveColour /></KpiValue>
                <KpiSub>{periodLabel}</KpiSub>
            </Kpi>
            <Kpi label="Expenses" tone="expense">
                <KpiValue><Amount amount={stats.expenses} currencyCode={account.currency} negativeColour zeroShowsAs="negative" /></KpiValue>
                <KpiSub>{periodLabel}</KpiSub>
            </Kpi>
            <Kpi label="Net" tone={netTone}>
                <KpiValue><Amount amount={stats.net} currencyCode={account.currency} plus minus positiveColour negativeColour zeroShowsAs="neutral" /></KpiValue>
                <KpiSub>{periodLabel}</KpiSub>
            </Kpi>
        </div>
    );
};

TransactionsCompactWidgets.displayName = "TransactionsCompactWidgets";
