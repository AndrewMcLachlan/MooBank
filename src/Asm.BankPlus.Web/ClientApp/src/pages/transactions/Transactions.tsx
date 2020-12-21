import React from "react";
import { RouteComponentProps, useParams } from "react-router";

import { TransactionList } from "./TransactionList";
import { AccountSummary } from "../../components";
import { usePageTitle } from "../../hooks";
import { useAccount } from "../../services";

export const Transactions: React.FC<TransactionsProps> = (props) => {

    usePageTitle("Transactions");

    const { id } = useParams<any>()

    const account = useAccount(id);

    if (!account.data) return (null);

    return (
        <>
            <AccountSummary account={account.data} />
            <TransactionList account={account.data} />
        </>);
}

export interface TransactionsProps extends RouteComponentProps<{ id: string }> {

}