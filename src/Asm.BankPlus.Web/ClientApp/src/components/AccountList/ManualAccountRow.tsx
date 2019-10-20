import React, { useState, useRef } from "react";
import { useDispatch } from "react-redux";
import { NavLink, useHistory } from "react-router-dom";

import { ActionTypes } from "../../store/Accounts";
import { AccountRowProps, useAccountRowCommonState } from "./AccountRow";
import { useClickAway } from "../../hooks";

export const ManualAccountRow: React.FC<AccountRowProps> = (props) => {

    const { balanceRef, avBalanceRef, editingBalance, editingAvBalance, balanceClick, avBalanceClick, } = useComponentState();
    const { onRowClick } = useAccountRowCommonState(props);

    return (
        <tr onClick={onRowClick} className="clickable">
            <td className="account">
                <div className="name">{props.account.name}</div>
            </td>
            <td onClick={balanceClick} ref={balanceRef}> {!editingBalance && <span className={props.account.currentBalance < 0 ? " negative" : ""}>{props.account.currentBalance + (props.account.currentBalance < 0 ? "D" : "C") + "R"}</span>}
                {editingBalance && <input type="number" value={props.account.currentBalance} />}
            </td>
            <td onClick={avBalanceClick} ref={avBalanceRef}> {!editingAvBalance && <span className={props.account.availableBalance < 0 ? " negative" : ""}>{props.account.availableBalance + (props.account.availableBalance < 0 ? "D" : "C") + "R"}</span>}
                {editingAvBalance && <input type="number" value={props.account.availableBalance} />}
            </td>
        </tr>
    );
}

ManualAccountRow.displayName = "ManualAccountRow";

const useComponentState = () => {

    const dispatch = useDispatch();
    const history = useHistory();

    const [editingBalance, setEditingBalance] = useState(false);
    const [editingAvBalance, setEditingAvBalance] = useState(false);

    const balanceRef = useRef(null);
    useClickAway(setEditingBalance, balanceRef);

    const avBalanceRef = useRef(null);
    useClickAway(setEditingAvBalance, avBalanceRef);

    const balanceClick = (e: React.MouseEvent<HTMLTableDataCellElement>) => {
        setEditingBalance(true);
        e.preventDefault();
        e.stopPropagation();
    };

    const avBalanceClick = (e: React.MouseEvent<HTMLTableDataCellElement>) => {
        setEditingAvBalance(true);
        e.preventDefault();
        e.stopPropagation();
    };

    return {
        balanceRef,
        avBalanceRef,

        editingBalance,
        editingAvBalance,

        balanceClick,
        avBalanceClick,
    };
}

