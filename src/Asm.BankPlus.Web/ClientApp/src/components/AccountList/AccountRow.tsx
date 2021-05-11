import classNames from "classnames";
import React from "react";
import { useHistory } from "react-router-dom";
import { getBalanceString, numberClassName } from "../../helpers";

import * as Models from "../../models";
import { AccountController, AccountType } from "../../models";

import { ManualAccountRow } from "./ManualAccountRow";

export const AccountRow: React.FC<AccountRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    switch (props.account.controller) {
        case AccountController.Manual:
            return <ManualAccountRow {...props} />
    }

    return (
        <tr onClick={onRowClick} className="clickable">
            <td>
               {props.account.name}
            </td>
            <td>
                {AccountType[props.account.accountType]}
            </td>
            <td className={classNames("number", numberClassName(props.account.currentBalance))}>{getBalanceString(props.account.currentBalance)}</td>
            {/*<td><span className={classNames("number", numberClassName(props.account.currentBalance))}>{getBalanceString(props.account.availableBalance)}</span></td>*/}
        </tr>
    );
}

AccountRow.displayName = "AccountRow";

export interface AccountRowProps {
    account: Models.Account;
}

export const useAccountRowCommonState = (props: AccountRowProps) => {

    var history = useHistory();

    const onRowClick = () => {
        history.push(`accounts/${props.account.id}`);
    };

    return {
        onRowClick,
    };
}

/*const useRenderers = (props: AccountRowProps) => {

    const renderManual = () => {

    }

    const renderImport = () => {

    }

    const getRenderer: Function = () => {
        switch (props.account.controller) {
            case AccountController.Manual:
                return renderManual;
                case AccountController.Import:
                    return renderImport;

        }

        return {
            useRenderers,
        };

    }
}*/