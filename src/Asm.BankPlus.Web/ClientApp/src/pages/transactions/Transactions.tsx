import React from "react";
import { RouteComponentProps } from "react-router";

import { TransactionList } from "./TransactionList";
import { AccountSummary } from "components";
import { useSelectedAccount, usePageTitle } from "hooks";

export const Transactions: React.FC<TransactionsProps> = (props) => {

    usePageTitle("Transactions");

    const accountId = props.match.params.id;

    const account = useSelectedAccount(accountId);

    return (
        <>
            <AccountSummary account={account} />
            <TransactionList account={account} />
        </>);
}

export interface TransactionsProps extends RouteComponentProps<{ id: string }> {

}