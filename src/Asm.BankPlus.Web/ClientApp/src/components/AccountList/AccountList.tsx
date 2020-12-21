import "./AccountList.scss";

import React from "react";
import { Table } from "react-bootstrap";

import { AccountRow } from "./AccountRow";
//import VirtualAccountRow from "./VirtualAccountRow";
import { getBalanceString } from "../../helpers";
import { useAccounts } from "../../services";

export const AccountList: React.FC<AccountListProps> = () => {

    const accountsQuery = useAccounts();

    const { data } = accountsQuery;

    const virtualAccountRows = [];

/*    if (virtualAccounts) {
        for (const account of virtualAccounts) {
            virtualAccountRows.push(<VirtualAccountRow key={account.virtualAccountId} account={account} />);
        }
    }*/

    const accountRows = [];
    if (data?.accounts) {
        for (const account of data?.accounts) {
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
                            {/*<th>Available Balance</th>*/}
                        </tr>
                    </thead>
                    <tbody>
                        {accountRows}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td>Position</td>
                            <td>{getBalanceString(data?.position)}</td>
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