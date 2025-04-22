import React from "react";
import { Spinner } from "react-bootstrap";

import { SectionTable } from "@andrewmclachlan/mooapp";
import { getBalanceString } from "helpers";
import { AccountListGroup as Model } from "models";
import { AccountRow } from "./AccountRow";

export interface AccountListGroupProps {
    group: Model;
    isLoading: boolean;
}

export const AccountListGroup: React.FC<AccountListGroupProps> = ({ group, isLoading }) => (
    <SectionTable className="accounts" hover header={group?.name} headerSize={2} hidden={group.instruments.length === 0}>
        <thead>
            <tr>
                <th className="expander d-none d-sm-table-cell"></th>
                <th>Name</th>
                <th className="d-none d-sm-table-cell">Type</th>
                <th className="number">Balance</th>
            </tr>
        </thead>
        <tbody>
            {//{!isLoading && group && group.instruments.map(a => a.controller === "Manual_BORK" ? <ManualAccountRow key={a.id} instrument={a} /> : <AccountRow key={a.id} instrument={a} />)}
            }
            {!isLoading && group && group.instruments.map(a => <AccountRow key={a.id} instrument={a} />)}
            {isLoading &&
                <tr><td colSpan={4} className="spinner">
                    <Spinner animation="border" />
                </td></tr>}
        </tbody>
        {group.total !== undefined && group.total !== null && <tfoot>
            <tr className="total">
                <td className="d-none d-sm-table-cell" />
                <td colSpan={2} className="d-none d-sm-table-cell">Total</td>
                <td className="d-table-cell d-sm-none"></td>
                <td className="amount number">{getBalanceString(group?.total)}</td>
            </tr>
        </tfoot>}
    </SectionTable>
);
