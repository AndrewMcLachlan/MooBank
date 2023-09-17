import React from "react";
import { Spinner, Table } from "react-bootstrap";

import { AccountController, AccountListGroup as Model } from "models";
import { ManualAccountRow } from "./ManualAccountRow";
import { AccountRow } from "./AccountRow";
import { getBalanceString } from "helpers";
import { Section } from "@andrewmclachlan/mooapp";

export interface AccountListGroupProps {
    accountGroup: Model;
    isLoading: boolean;
}

export const AccountListGroup: React.FC<AccountListGroupProps> = ({ accountGroup, isLoading }) => (
    <Section title={accountGroup?.name} size={2} hidden={accountGroup.accounts.length === 0}>
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
                {!isLoading && accountGroup && accountGroup.accounts.map(a => a.controller === "Manual" ? <ManualAccountRow key={a.id} account={a} /> : <AccountRow key={a.id} account={a} />)}
                {isLoading &&
                    <tr><td colSpan={4} className="spinner">
                        <Spinner animation="border" />
                    </td></tr>}
            </tbody>
            { accountGroup.position && <tfoot>
                <tr className="position">
                    <td />
                    <td colSpan={2}>Position</td>
                    <td className="amount number">{getBalanceString(accountGroup?.position)}</td>
                </tr>
            </tfoot>}
        </Table>
    </Section>
)