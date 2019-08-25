﻿import React from "react";

import * as Models from "../models";

export const AccountRow :React.FC<AccountRowProps> = (props) =>
(
<tr>
    <td className="account">
        <div className="name">{props.account.name}</div>
        <nav className="desktop">
            <ul>
                <li>
                    <a href="">Update Balance</a>
                </li>
                <li>
                    <a href="">Save</a>
                </li>
                <li>
                    <a href="">Cancel</a>
                </li>
            </ul>
        </nav>
    </td>
    <td><span className={props.account.currentBalance < 0 ? " negative" : ""}>{props.account.currentBalance + (props.account.currentBalance < 0 ? "D" : "C") + "R"}</span><input  type="number" value={props.account.currentBalance} /></td>
    <td><span className={props.account.availableBalance < 0 ? " negative" : ""}>{props.account.availableBalance + (props.account.availableBalance < 0 ? "D" : "C") + "R"}</span> <input type="number" value={props.account.availableBalance} /></td>
</tr>
);

export interface AccountRowProps {
    account: Models.Account;
}