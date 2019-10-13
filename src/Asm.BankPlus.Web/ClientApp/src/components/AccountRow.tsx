import React from "react";

import * as Models from "../models";
import { NavLink } from "react-router-dom";
import { ActionTypes } from "../store/Accounts";
import { useDispatch } from "react-redux";
import { AccountController } from "../models";

import { ManualAccountRow} from "./ManualAccountRow";

export const AccountRow: React.FC<AccountRowProps> = (props) => {

    const dispatch = useDispatch();

    switch (props.account.controller) {
        case AccountController.Manual:
            return <ManualAccountRow {...props} />
        case AccountController.Import:
    }

    return (
        <tr>
            <td className="account">
                <div className="name"><NavLink onClick={() => dispatch({ type: ActionTypes.SetSelectedAccount, data: props.account })} to={"accounts/" + props.account.id}>{props.account.name}</NavLink></div>
            </td>
            <td><span className={props.account.currentBalance < 0 ? " negative" : ""}>{props.account.currentBalance + (props.account.currentBalance < 0 ? "D" : "C") + "R"}</span></td>
            <td><span className={props.account.availableBalance < 0 ? " negative" : ""}>{props.account.availableBalance + (props.account.availableBalance < 0 ? "D" : "C") + "R"}</span></td>
        </tr>
    );
}

AccountRow.displayName = "AccountRow";

export interface AccountRowProps {
    account: Models.Account;
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