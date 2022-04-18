import React from "react";
import { useNavigate } from "react-router-dom";

import * as Models from "../../models";
import { AccountController, AccountType } from "../../models";

export const AccountRow: React.FC<AccountRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    return (
        <tr onClick={onRowClick} className="clickable">
            <td className="account">
                <div className="name">{props.account.name}</div>
            </td>
            <td>
                {props.account.description}
            </td>
            <td>
                {AccountType[props.account.accountType]}
            </td>
            <td>{AccountController[props.account.controller]}</td>
            <td className="number"><span className={props.account.currentBalance < 0 ? " negative" : ""}>{props.account.currentBalance + (props.account.currentBalance < 0 ? "D" : "C") + "R"}</span></td>
            {/*}            <td><span className={props.account.availableBalance < 0 ? " negative" : ""}>{props.account.availableBalance + (props.account.availableBalance < 0 ? "D" : "C") + "R"}</span></td>*/}
        </tr>
    );
}

AccountRow.displayName = "AccountRow";

export interface AccountRowProps {
    account: Models.Account;
}

export const useAccountRowCommonState = (props: AccountRowProps) => {

    var navigate = useNavigate();

    const onRowClick = () => {
        navigate(`/accounts/${props.account.id}/manage`);
    };

    return {
        onRowClick,
    };
}
