import React from "react";
import { RouteComponentProps, useParams } from "react-router";

import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";
import { AccountHeader, AccountProvider, AccountSummary } from "../../components";
import { useAccount } from "../../services";
import { Page } from "../../layouts";

export const Transactions: React.FC<TransactionsProps> = () => {

    const { id } = useParams<any>()

    const account = useAccount(id);

    if (!account.data) return (null);

    return (
        <Page title={account?.data?.name}>
            <AccountProvider account={account.data}>
                <AccountHeader />
                <Page.Content>
                    <div className="transaction-list-header">
                        <AccountSummary />
                        <FilterPanel />
                    </div>
                    <TransactionList />
                </Page.Content>
            </AccountProvider>
        </Page>
    );
}

export interface TransactionsProps extends RouteComponentProps<{ id: string }> {

}