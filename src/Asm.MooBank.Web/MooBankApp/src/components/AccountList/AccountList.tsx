import "./AccountList.scss";

import React from "react";
import { Spinner, Table } from "react-bootstrap";

import { AccountRow } from "./AccountRow";
import { getBalanceString } from "../../helpers";
import { useFormattedAccounts } from "../../services";
import { AccountController } from "../../models";
import { ManualAccountRow } from "./ManualAccountRow";

export const AccountList: React.FC<AccountListProps> = () => {

    const { data, isLoading } = useFormattedAccounts();

    const hasBoth = (data?.positionedAccounts?.length ?? 0) > 0 && (data?.otherAccounts?.length ?? 0) > 0;

    return (
        <>
            <section className="account-list">
                {hasBoth && <h2>Main Accounts</h2>}
                <Table className="accounts" hover>
                    <thead>
                        <tr>
                            <th className="expander"></th>
                            <th>Name</th>
                            <th className="type">Type</th>
                            <th className="number">Current Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {!isLoading && data?.positionedAccounts && data.positionedAccounts.map(a => a.controller === AccountController.Manual ? <ManualAccountRow key={a.id} account={a} /> : <AccountRow key={a.id} account={a} />)}
                        {isLoading &&
                            <tr><td colSpan={4} className="spinner">
                                <Spinner animation="border" />
                            </td></tr>}
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

            <section className="account-list">
                {hasBoth && <h2>Other Accounts</h2>}
                <Table className="accounts" hover>
                    <thead>
                        <tr>
                            <th className="expander"></th>
                            <th>Name</th>
                            <th className="type">Type</th>
                            <th className="number">Current Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {!isLoading && data?.otherAccounts && data.otherAccounts.map(a => a.controller === AccountController.Manual ? <ManualAccountRow key={a.id} account={a} /> : <AccountRow key={a.id} account={a} />)}
                        {isLoading &&
                            <tr><td colSpan={4} className="spinner">
                                <Spinner animation="border" />
                            </td></tr>}
                    </tbody>
                </Table>
            </section>
        </>
    );
}

AccountList.displayName = "AccountList";

export interface AccountListProps {

}