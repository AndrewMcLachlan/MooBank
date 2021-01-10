import React, { useState, useRef } from "react";
import { getBalanceString } from "../../helpers";

import { AccountRowProps } from "./AccountRow";
import { useClickAway } from "../../hooks";
import { AccountType } from "../../models";

export const ManualAccountRow: React.FC<AccountRowProps> = (props) => {

    const { balanceRef, editingBalance, balanceClick, balanceChange, balance } = useComponentState(props);

    return (
        <tr onClick={balanceClick} className="clickable" ref={balanceRef}>
            <td className="account">
                <div className="name">{props.account.name}</div>
            </td>
            <td>
                {AccountType[props.account.accountType]}
            </td>
            <td> {!editingBalance && <span className={props.account.currentBalance < 0 ? " negative" : ""}>{getBalanceString(balance)}</span>}
                {editingBalance && <input type="number" value={balance} onChange={balanceChange} />}
            </td>
            {/*<td onClick={avBalanceClick} ref={avBalanceRef}> {!editingAvBalance && <span className={props.account.availableBalance < 0 ? " negative" : ""}>{getBalanceString(avBalance)}</span>}
                {editingAvBalance && <input type="number" value={avBalance} onChange={avBalanceChange} />}
            </td>*/}
        </tr>
    );
}

ManualAccountRow.displayName = "ManualAccountRow";

const useComponentState = (props: AccountRowProps) => {

    const [editingBalance, setEditingBalance] = useState(false);

    const [balance, setBalance] = useState(props.account.currentBalance);

    const balanceRef = useRef(null);
    useClickAway(setEditingBalance, balanceRef);

    const balanceClick = (e: React.MouseEvent<HTMLTableRowElement>) => {
        setEditingBalance(true);
        e.preventDefault();
        e.stopPropagation();
    };

    const balanceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setBalance(parseFloat(e.currentTarget.value));
    }


    return {
        balanceRef,

        editingBalance,

        balanceClick,

        balanceChange,

        balance,
    };
}

