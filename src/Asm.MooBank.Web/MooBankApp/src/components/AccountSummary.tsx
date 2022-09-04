import "./AccountSummary.scss";

import React from "react";

import { AccountBalance } from ".";
import { useAccount } from "./AccountProvider";
import { Table } from "react-bootstrap";

export const AccountSummary: React.FC<AccountSummaryProps> = (props) => {

    const account = useAccount();

    if (!account) return null;

    return (
        <Table className="account-summary">
            <tbody>
                <tr>
                    <th>Balance</th>
                    <td className="balance"><AccountBalance balance={account.currentBalance} /></td>
                </tr>
                {/*<tr>
                        <th>Available Balance</th>
                        <td><AccountBalance balance={account.availableBalance} /></td>
                    </tr>*/}
            </tbody>
        </Table>
    )
}

AccountSummary.displayName = "AccountSummary";

export interface AccountSummaryProps {
}