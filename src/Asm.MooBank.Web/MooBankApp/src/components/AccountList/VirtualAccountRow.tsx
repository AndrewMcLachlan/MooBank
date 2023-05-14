import classNames from "classnames";
import React, { useEffect, useRef, useState } from "react";

import { getBalanceString, numberClassName } from "helpers";
import { emptyGuid, useClickAway } from "@andrewmclachlan/mooapp";

import { accountId, VirtualAccount } from "models";
import { useUpdateVirtualAccountBalance } from "services";

export const VirtualAccountRow: React.FC<VirtualAccountRowProps> = (props) => {
    const { balanceRef, editingBalance, balanceClick, balanceChange, balance, keyPress } = useComponentState(props);

    return (
        <tr onClick={balanceClick} className="virtual clickable" ref={balanceRef}>
            <td></td>
            <td className="name">{props.account.name}</td>
            <td>Virtual</td>
            {/*<nav className="desktop">
                        <ul>
                            <li>
                                <a href={"/transactions/transfer/" + this.props.account.id}>Transfer funds</a>
                            </li>
                            <li>
                                <a href={"/transactions/history/" + this.props.account.id}>Transactions</a>
                            </li>
                        </ul>
        </nav>*/}
            <td className={classNames("amount", "number", numberClassName(balance))}>
                {!editingBalance && getBalanceString(balance)}
                {editingBalance && <input type="number" value={balance} onChange={balanceChange} onKeyPress={keyPress} />}
            </td>
        </tr>
    );
}

interface VirtualAccountRowProps {
    account: VirtualAccount;
    accountId: accountId;
}

VirtualAccountRow.displayName = "VirtualAccountRow";

const useComponentState = (props: VirtualAccountRowProps) => {

    const [editingBalance, setEditingBalance] = useState(false);

    const [balance, setBalance] = useState(props.account.balance);

    const updateBalance = useUpdateVirtualAccountBalance();

    const balanceRef = useRef(null);

    useEffect(() => {
        setBalance(props.account.balance);
    }, [props]);

    useClickAway(setEditingBalance, balanceRef, () => (editingBalance && props.account.balance !== balance) && updateBalance.update(props.accountId, props.account.id, balance));

    const balanceClick = (e: React.MouseEvent<HTMLTableRowElement>) => {
        setEditingBalance(props.account.id !== emptyGuid);
        e.preventDefault();
        e.stopPropagation();
    };

    const balanceChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        setBalance(parseFloat(e.currentTarget.value));
    }

    const keyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            updateBalance.update(props.accountId, props.account.id, balance);
            setEditingBalance(false);
        }
    }

    return {
        balanceRef,

        editingBalance,

        balanceClick,

        balanceChange,

        balance,

        keyPress,
    };
}
