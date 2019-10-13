import React from "react";

import { Account } from "../models";
import { AccountBalance } from ".";

export const AccountSummary: React.FC<AccountSummaryProps> = (props) => {

    if (!props.account) return null;

    return (
        <>
            <h1>{props.account.name}</h1>

            <table className="table">
                <tbody>
                    <tr>
                        <th>Balance</th>
                        <td><AccountBalance balance={props.account.currentBalance} /></td>
                    </tr>
                    <tr>
                        <th>Available Balance</th>
                        <td><AccountBalance balance={props.account.availableBalance} /></td>
                    </tr>
                </tbody>
            </table>
        </>
    )
}

AccountSummary.displayName = "AccountSummary";

export interface AccountSummaryProps {
    account: Account;
}