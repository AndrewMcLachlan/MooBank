import React from "react";
import { useParams } from "react-router-dom";

import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";
import { AccountHeader, AccountProvider, AccountSummary } from "components";
import { useAccount } from "services";
import { Page } from "layouts";
import { useIdParams } from "hooks";

export const Transactions: React.FC = () => {

    const id = useIdParams();

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
                    <TransactionList account={account.data} />
                </Page.Content>
            </AccountProvider>
        </Page>
    );
}
