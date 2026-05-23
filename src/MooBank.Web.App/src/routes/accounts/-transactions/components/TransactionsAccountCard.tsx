import React from "react";
import { Section } from "@andrewmclachlan/moo-ds";

import { Amount, useAccount } from "components";
import type { LogicalAccount } from "api/types.gen";
import { formatDate } from "utils/dateFns";

import { usePeriodLabel } from "../hooks/usePeriodLabel";
import { useTransactionPeriodStats } from "../hooks/useTransactionPeriodStats";

export const TransactionsAccountCard: React.FC = () => {

    const account = useAccount();
    const stats = useTransactionPeriodStats(account?.id ?? "");
    const periodLabel = usePeriodLabel();

    if (!account) return null;

    const balance = (account as LogicalAccount).currentBalanceLocalCurrency ?? account.currentBalance ?? 0;
    const instrumentType = (account as LogicalAccount).instrumentType ?? "Virtual";

    return (
        <Section className="tx-account-card accent-stripe">
            <div className="eyebrow">{account.name} · Balance</div>
            <div className="hero-balance"><Amount amount={balance} currencyCode={account.currency} minus /></div>
            <div className="hero-subline">
                Last transaction <span className="strong">{formatDate(account.lastTransaction)}</span>
                <span className="sep">·</span>
                Type <span className="strong">{instrumentType}</span>
            </div>

            <div className="period-block">
                <div className="period-head">
                    <span className="eyebrow">{periodLabel}</span>
                    <span className="period-count">{stats.total} {stats.total === 1 ? "transaction" : "transactions"}</span>
                </div>

                <div className="stat-grid">
                    <div className="stat-block">
                        <div className="lbl">Income</div>
                        <div className="val"><Amount amount={stats.income} currencyCode={account.currency} positiveColour /></div>
                    </div>
                    <div className="stat-block">
                        <div className="lbl">Expenses</div>
                        <div className="val"><Amount amount={-stats.expenses} currencyCode={account.currency} negativeColour zeroShowsAs="negative" /></div>
                    </div>
                    <div className="stat-block">
                        <div className="lbl">Net</div>
                        <div className="val"><Amount amount={stats.net} currencyCode={account.currency} plus minus positiveColour negativeColour zeroShowsAs="neutral" /></div>
                    </div>
                </div>
            </div>
        </Section>
    );
};

TransactionsAccountCard.displayName = "TransactionsAccountCard";
