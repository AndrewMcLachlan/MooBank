import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { actionCreators } from "../store/Accounts";
import { State } from "../store/state";
import { AccountRow } from "../components";
import VirtualAccountRow from "../components/VirtualAccountRow";

export const Home: React.FC = (props) => {

    const dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    useEffect(() => {
        dispatch(actionCreators.requestAccounts());
    }, [props,dispatch]);

    const accounts = useSelector((state: State) => state.accounts.accounts);
    const virtualAccounts = useSelector((state: State) => state.accounts.virtualAccounts);

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
        <section>

            <h2>Accounts</h2>

            <table className="accounts">
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
            </table>

            <h2>Virtual Accounts</h2>

            <table id="virtualAccounts" className="accounts">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Balance</th>
                    </tr>
                </thead>
                <tbody>
                    {virtualAccountRows}
                </tbody>
            </table>
        </section>
    );
}
