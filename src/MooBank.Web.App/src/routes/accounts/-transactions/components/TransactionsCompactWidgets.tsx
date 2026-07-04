import React from "react";
import { Section } from "@andrewmclachlan/moo-ds";

import { Amount, useAccount } from "components";
import type { LogicalAccount } from "api/types.gen";
import { formatDate } from "utils/dateFns";

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
            <Section className="widget" data-tone="balance">
                <div className="eyebrow">{account.name} · Balance</div>
                <div className="widget-value strong"><Amount amount={balance} currencyCode={account.currency} minus /></div>
                <div className="widget-sub">Last tx · {formatDate(account.lastTransaction)}</div>
            </Section>
            <Section className="widget" data-tone="income">
                <div className="eyebrow">Income</div>
                <div className="widget-value"><Amount amount={stats.income} currencyCode={account.currency} positiveColour /></div>
                <div className="widget-sub">{periodLabel}</div>
            </Section>
            <Section className="widget" data-tone="expense">
                <div className="eyebrow">Expenses</div>
                <div className="widget-value"><Amount amount={stats.expenses} currencyCode={account.currency} negativeColour zeroShowsAs="negative" /></div>
                <div className="widget-sub">{periodLabel}</div>
            </Section>
            <Section className="widget" data-tone={netTone}>
                <div className="eyebrow">Net</div>
                <div className="widget-value"><Amount amount={stats.net} currencyCode={account.currency} plus minus positiveColour negativeColour zeroShowsAs="neutral" /></div>
                <div className="widget-sub">{periodLabel}</div>
            </Section>
        </div>
    );
};

TransactionsCompactWidgets.displayName = "TransactionsCompactWidgets";
