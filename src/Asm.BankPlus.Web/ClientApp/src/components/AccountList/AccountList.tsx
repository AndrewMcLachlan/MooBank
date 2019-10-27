import "./AccountList.scss";

import React, { useEffect } from "react";
import { Table } from "react-bootstrap";
import { useSelector, useDispatch } from "react-redux";
import { bindActionCreators } from "redux";

import { actionCreators } from "../../store/Accounts";

import { AccountRow } from "./AccountRow";
import VirtualAccountRow from "./VirtualAccountRow";
import { State } from "../../store/state";

export const AccountList: React.FC<AccountListProps> = () => {
    const dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    useEffect(() => {
        dispatch(actionCreators.requestAccounts());
    }, [dispatch]);

    const accountsState = useSelector((state: State) => state.accounts);

    const { accounts, virtualAccounts, position } = accountsState;


    const virtualAccountRows = [];

    if (virtualAccounts) {
        for (const account of virtualAccounts) {
            virtualAccountRows.push(<VirtualAccountRow key={account.virtualAccountId} account={account} />);
        }
    }

    const accountRows = [];
    if (accounts) {
        for (const account of accounts) {
            accountRows.push(<AccountRow key={account.id} account={account} />);
        }
    }

    return (
        <>
            <section className="account-list">
                <h2>Accounts</h2>

                <Table className="accounts" hover>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Current Balance</th>
                            <th>Available Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {accountRows}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td>Position</td>
                            <td colSpan={2}></td>
                        </tr>
                    </tfoot>
                </Table>
            </section>
           {/* <section className="account-list">
                <h2>Virtual Accounts</h2>

                <Table id="virtualAccounts" className="accounts">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {virtualAccountRows}
                    </tbody>
                </Table>
    </section>*/}
        </>
    );
}

AccountList.displayName = "AccountList";

export interface AccountListProps {

}