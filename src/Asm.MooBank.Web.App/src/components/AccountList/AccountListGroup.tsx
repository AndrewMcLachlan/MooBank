import React from "react";
import { Spinner, Table } from "react-bootstrap";

import { Section } from "@andrewmclachlan/mooapp";
import { getBalanceString } from "helpers";
import { AccountListGroup as Model } from "models";
import { AccountRow } from "./AccountRow";
import { ManualAccountRow } from "./ManualAccountRow";

export interface AccountListGroupProps {
    accountGroup: Model;
    isLoading: boolean;
}

export const AccountListGroup: React.FC<AccountListGroupProps> = ({ accountGroup, isLoading }) => (
    <Section title={accountGroup?.name} size={2} hidden={accountGroup.accounts.length === 0}>
        <Table className="accounts" hover>
            <thead>
                <tr>
                    <th className="expander d-none d-sm-table-cell"></th>
                    <th>Name</th>
                    <th className="d-none d-sm-table-cell">Type</th>
                    <th className="number">Current Balance</th>
                </tr>
            </thead>
            <tbody>
                {!isLoading && accountGroup && accountGroup.accounts.map(a => a.controller === "Manual" ? <ManualAccountRow key={a.id} account={a} /> : <AccountRow key={a.id} account={a} />)}
                {isLoading &&
                    <tr><td colSpan={4} className="spinner">
                        <Spinner animation="border" />
                    </td></tr>}
            </tbody>
            { accountGroup.position !== undefined && accountGroup.position !== null && <tfoot>
                <tr className="position">
                    <td className="d-none d-sm-table-cell" />
                    <td colSpan={2} className="d-none d-sm-table-cell">Position</td>
                    <td className="d-table-cell d-sm-none"></td>
                    <td className="amount number">{getBalanceString(accountGroup?.position)}</td>
                </tr>
            </tfoot>}
        </Table>
    </Section>
)