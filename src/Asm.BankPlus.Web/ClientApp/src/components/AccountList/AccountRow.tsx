import React from "react";
import { useHistory } from "react-router-dom";

import * as Models from "../../models";
import { AccountController } from "../../models";

import { ManualAccountRow} from "./ManualAccountRow";

export const AccountRow: React.FC<AccountRowProps> = (props) => {

const { onRowClick } = useAccountRowCommonState(props);

    switch (props.account.controller) {
        case AccountController.Manual:
            return <ManualAccountRow {...props} />
        case AccountController.Import:
    }

    return (
        <tr onClick={onRowClick} className="clickable">
            <td className="account">
                <div className="name">{props.account.name}</div>
            </td>
            <td><span className={props.account.currentBalance < 0 ? " negative" : ""}>{props.account.currentBalance + (props.account.currentBalance < 0 ? "D" : "C") + "R"}</span></td>
{/*}            <td><span className={props.account.availableBalance < 0 ? " negative" : ""}>{props.account.availableBalance + (props.account.availableBalance < 0 ? "D" : "C") + "R"}</span></td>*/}
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