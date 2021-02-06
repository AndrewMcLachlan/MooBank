import React from "react";
import { RouteComponentProps, useParams } from "react-router";

import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";
import { AccountHeader, AccountProvider, AccountSummary } from "../../components";
import { usePageTitle } from "../../hooks";
import { useAccount } from "../../services";

export const Transactions: React.FC<TransactionsProps> = (props) => {

    usePageTitle("Transactions");

    const { id } = useParams<any>()

    const account = useAccount(id);

    if (!account.data) return (null);

    return (
        <AccountProvider account={account.data}>
            <AccountHeader />
            <div className="transaction-list-header">
                <AccountSummary />
                <FilterPanel />
            </div>
            <TransactionList />
        </AccountProvider>);
}

export interface TransactionsProps extends RouteComponentProps<{ id: string }> {

}