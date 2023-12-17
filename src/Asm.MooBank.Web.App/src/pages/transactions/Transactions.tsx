import React from "react";

import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";
import { AccountPage, AccountProvider, AccountSummary } from "../../components";
import { useAccount } from "components";
import { AccountType } from "models";

export const Transactions: React.FC = () => {

    return (
        <AccountPage title="Transactions">
            <div className="section-group transactions-header">
                <AccountSummary />
                <FilterPanel />
            </div>
            <TransactionList />
        </AccountPage>
    );
}
