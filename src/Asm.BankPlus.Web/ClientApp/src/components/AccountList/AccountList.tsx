import "./AccountList.scss";

import React, { useEffect, useState } from "react";
import { Spinner, Table } from "react-bootstrap";

import { AccountRow } from "./AccountRow";
import { emptyGuid, getBalanceString } from "../../helpers";
import { useAccounts } from "../../services";
import { AccountController, Accounts } from "../../models";
import { ManualAccountRow } from "./ManualAccountRow";
import { useHttpClient } from "../HttpClientProvider";
import { useQuery } from "react-query";

export const AccountList: React.FC<AccountListProps> = () => {

    const httpClient = useHttpClient();

    const { data, isLoading } = useAccounts();

    return (

        <section className="account-list">
            <h1>Accounts</h1>

            <Table className="accounts" hover>
                <thead>
                    <tr>
                        <th></th>
                        <th>Name</th>
                        <th className="type">Type</th>
                        <th className="number">Current Balance</th>
                        {/*<th className="number">Available Balance</th>*/}
                    </tr>
                </thead>
                <tbody>
                    {!isLoading && data?.accounts && data.accounts.map(a => a.controller === AccountController.Manual ? <ManualAccountRow key={a.id} account={a} /> : <AccountRow key={a.id} account={a} />)}
                    {isLoading && <Spinner animation="border" />}
                </tbody>
                <tfoot>
                    <tr className="position">
                        <td />
                        <td colSpan={2}>Position</td>
                        <td className="number">{getBalanceString(data?.position)}</td>
                    </tr>
                </tfoot>
            </Table>
        </section>
    );
}

AccountList.displayName = "AccountList";

export interface AccountListProps {

}