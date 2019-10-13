import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RouteComponentProps } from "react-router";
import { bindActionCreators } from "redux";

import { actionCreators as accountActionCreators } from "store/Accounts";
import { State } from "store/state";

import { TransactionList, AccountSummary } from "components";
import { useSelectedAccount } from "hooks";

export const Transactions: React.FC<TransactionsProps> = (props) => {

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