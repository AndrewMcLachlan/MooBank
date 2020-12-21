import React, { useState, useRef } from "react";
import { getBalanceString } from "../../helpers";

import { AccountRowProps, useAccountRowCommonState } from "./AccountRow";
import { useClickAway } from "../../hooks";

export const ManualAccountRow: React.FC<AccountRowProps> = (props) => {

    const { balanceRef, avBalanceRef, editingBalance, editingAvBalance, balanceClick, avBalanceClick, balanceChange, avBalanceChange, balance, avBalance } = useComponentState(props);
    const { onRowClick } = useAccountRowCommonState(props);

    return (
        <tr onClick={onRowClick} className="clickable">
            <td className="account">
                <div className="name">{props.account.name}</div>
            </td>
            <td onClick={balanceClick} ref={balanceRef}> {!editingBalance && <span className={props.account.currentBalance < 0 ? " negative" : ""}>{getBalanceString(balance)}</span>}
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

    //const dispatch = useDispatch();
    //const history = useHistory();

    const [editingBalance, setEditingBalance] = useState(false);
    const [editingAvBalance, setEditingAvBalance] = useState(false);

    const [balance, setBalance] = useState(props.account.currentBalance);
    const [avBalance, setAvBalance] = useState(props.account.availableBalance);

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

    const balanceChange =(e: React.ChangeEvent<HTMLInputElement>) => {
        setBalance(parseFloat(e.currentTarget.value));
    }

    const avBalanceChange =(e: React.ChangeEvent<HTMLInputElement>) => {
        setAvBalance(parseFloat(e.currentTarget.value));
    }

    return {
        balanceRef,
        avBalanceRef,

        editingBalance,
        editingAvBalance,

        balanceClick,
        avBalanceClick,

        balanceChange,
        avBalanceChange,

        balance,
        avBalance,
    };
}

