import React from "react";

import { Kpi } from "@andrewmclachlan/moo-ds";

import { Amount, useAccount } from "components";
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
                <Kpi.Value className="strong"><Amount amount={balance} currencyCode={account.currency} minus /></Kpi.Value>
                <Kpi.Sub>Last tx · {formatDisplayDate(account.lastTransaction)}</Kpi.Sub>
            </Kpi>
            <Kpi label="Income" tone="income">
                <Kpi.Value><Amount amount={stats.income} currencyCode={account.currency} positiveColour /></Kpi.Value>
                <Kpi.Sub>{periodLabel}</Kpi.Sub>
            </Kpi>
            <Kpi label="Expenses" tone="expense">
                <Kpi.Value><Amount amount={stats.expenses} currencyCode={account.currency} negativeColour zeroShowsAs="negative" /></Kpi.Value>
                <Kpi.Sub>{periodLabel}</Kpi.Sub>
            </Kpi>
            <Kpi label="Net" tone={netTone}>
                <Kpi.Value><Amount amount={stats.net} currencyCode={account.currency} plus minus positiveColour negativeColour zeroShowsAs="neutral" /></Kpi.Value>
                <Kpi.Sub>{periodLabel}</Kpi.Sub>
            </Kpi>
        </div>
    );
};

TransactionsCompactWidgets.displayName = "TransactionsCompactWidgets";
