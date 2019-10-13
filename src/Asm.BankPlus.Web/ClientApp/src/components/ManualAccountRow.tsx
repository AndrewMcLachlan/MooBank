import React, { useState, useRef } from "react";

import * as Models from "../models";
import { NavLink } from "react-router-dom";
import { ActionTypes } from "../store/Accounts";
import { useDispatch } from "react-redux";
import { ActionWithData } from "../store/redux-extensions";
import { AccountRowProps } from "./AccountRow";
import { useClickAway } from "hooks";

export const ManualAccountRow: React.FC<AccountRowProps> = (props) => {

    const dispatch = useDispatch();

    const [editingBalance, setEditingBalance] = useState(false);
    const [editingAvBalance, setEditingAvBalance] = useState(false);

    const balanceRef = useRef(null);
    useClickAway(setEditingBalance, balanceRef);

    const avBalanceRef = useRef(null);
    useClickAway(setEditingAvBalance, avBalanceRef);

    return (
        <tr>
            <td className="account">
                <div className="name"><NavLink onClick={() => dispatch({ type: ActionTypes.SetSelectedAccount, data: props.account })} to={"accounts/" + props.account.id}>{props.account.name}</NavLink></div>
            </td>
            <td onClick={() => setEditingBalance(true)} ref={balanceRef}> {!editingBalance && <span className={props.account.currentBalance < 0 ? " negative" : ""}>{props.account.currentBalance + (props.account.currentBalance < 0 ? "D" : "C") + "R"}</span>}
                {editingBalance && <input type="number" value={props.account.currentBalance} />}
            </td>
            <td onClick={() => setEditingAvBalance(true)} ref={avBalanceRef}> {!editingAvBalance && <span className={props.account.availableBalance < 0 ? " negative" : ""}>{props.account.availableBalance + (props.account.availableBalance < 0 ? "D" : "C") + "R"}</span>}
                {editingAvBalance && <input type="number" value={props.account.availableBalance} />}
            </td>
        </tr>
    );
}