import React from "react";

import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";
import { AccountPage, AccountProvider, AccountSummary } from "../../components";
import { useAccount } from "components";

export const Transactions: React.FC = () => {

    const account = useAccount();

    return (
        <AccountPage title="Transactions">
            <div className="section-group transactions-header">
                <AccountSummary />
                <FilterPanel />
            </div>
            <TransactionList account={account} />
        </AccountPage>
    );
}
