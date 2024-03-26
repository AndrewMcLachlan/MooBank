import classNames from "classnames";
import React, { useEffect, useRef, useState } from "react";

import { getBalanceString, numberClassName } from "helpers";
import { emptyGuid, useClickAway } from "@andrewmclachlan/mooapp";

import { AccountId, VirtualAccount } from "models";
import { useUpdateVirtualAccountBalance } from "services";
import { useNavigate } from "react-router-dom";

export const VirtualAccountRow: React.FC<VirtualAccountRowProps> = (props) => {
    const { balanceRef, editingBalance, balanceClick, balanceChange, balance, keyPress, onRowClick } = useComponentState(props);

    const clickable = props.account.id !== emptyGuid;

    return (
        <tr onClick={clickable ? onRowClick : undefined} className={classNames("virtual", clickable && "clickable", "d-none", "d-sm-table-row")} ref={balanceRef}>
            <td></td>
            <td className="name">{props.account.name}</td>
            <td className="d-none d-sm-table-cell">Virtual</td>
            <td className={classNames("amount", "number", numberClassName(balance))} onClick={clickable ? balanceClick : undefined}>
                {!editingBalance && getBalanceString(balance)}
                {editingBalance && <input type="number" value={balance} onChange={balanceChange} onKeyUp={keyPress} />}
            </td>
        </tr>
    );
}

interface VirtualAccountRowProps {
    account: VirtualAccount;
    accountId: AccountId;
}

VirtualAccountRow.displayName = "VirtualAccountRow";

const useComponentState = (props: VirtualAccountRowProps) => {

    const navigate = useNavigate();

    const [editingBalance, setEditingBalance] = useState(false);
    const [balance, setBalance] = useState(props.account.currentBalance);

    const updateBalance = useUpdateVirtualAccountBalance();
    const balanceRef = useRef(null);

    useEffect(() => {
        setBalance(props.account.currentBalance);
    }, [props]);

    useClickAway(setEditingBalance, balanceRef, () => (editingBalance && props.account.currentBalance !== balance) && updateBalance(props.accountId, props.account.id, balance));

    const balanceClick = (e: React.MouseEvent<HTMLTableCellElement>) => {
        setEditingBalance(props.account.id !== emptyGuid);
        e.preventDefault();
        e.stopPropagation();
    };

    const balanceChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        setBalance(parseFloat(e.currentTarget.value));
    }

    const keyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            updateBalance(props.accountId, props.account.id, balance);
            setEditingBalance(false);
        }
    }

    const onRowClick = () => {
        navigate(`/accounts/${props.accountId}/virtual/${props.account.id}`);
    };

    return {
        balanceRef,

        editingBalance,

        balanceClick,

        balanceChange,

        balance,

        keyPress,

        onRowClick,
    };
}
