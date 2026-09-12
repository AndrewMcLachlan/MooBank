import React from "react";

import { Amount, useAccount } from "components";
import type { LogicalAccount } from "api/types.gen";

import { useTransactionPeriodStats } from "../hooks/useTransactionPeriodStats";

/**
 * The transactions page header on a phone. The account name is the app bar
 * title and the period is the filter bar's own control, so neither is repeated
 * here; what is left is where the account stands and which way the selected
 * period moved it.
 */
export const TransactionsMobileHeader: React.FC = () => {

    const account = useAccount();
    const stats = useTransactionPeriodStats(account?.id ?? "");

    if (!account) return null;

    const balance = (account as LogicalAccount).currentBalance ?? 0;

    return (
        <section className="tx-mobile-header">
            <div className="tx-mobile-figure">
                <div className="lbl">Balance</div>
                <div className="val balance"><Amount amount={balance} currencyCode={account.currency} minus /></div>
            </div>
            <div className="tx-mobile-figure net">
                <div className="lbl">Net</div>
                <div className="val"><Amount amount={stats.net} currencyCode={account.currency} plus minus positiveColour negativeColour zeroShowsAs="neutral" /></div>
            </div>
        </section>
    );
};

TransactionsMobileHeader.displayName = "TransactionsMobileHeader";
