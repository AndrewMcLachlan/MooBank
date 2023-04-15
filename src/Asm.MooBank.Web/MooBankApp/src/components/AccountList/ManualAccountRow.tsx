import React, { useState, useRef } from "react";
import { getBalanceString, numberClassName } from "helpers";

import { AccountRowProps } from "./AccountRow";
import { useClickAway } from "hooks";
import { AccountType } from "models";
import { useUpdateBalance } from "services";
import classNames from "classnames";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { VirtualAccountRow } from "./VirtualAccountRow";
import { MD5 } from "helpers";

export const ManualAccountRow: React.FC<AccountRowProps> = (props) => {

    const { balanceRef, editingBalance, balanceClick, balanceChange, balance, keyPress } = useComponentState(props);

    const [showVirtualAccounts, setShowVirtualAccounts] = useState<Boolean>(localStorage.getItem(`account|${MD5(props.account.id)}`) === "true");

    const showVirtualAccountsClick = (e: React.MouseEvent<HTMLTableDataCellElement>) => {
        e.preventDefault();
        e.stopPropagation();
        setShowVirtualAccounts(!showVirtualAccounts);

        localStorage.setItem(`account|${MD5(props.account.id)}`, (!showVirtualAccounts).toString());
    }

    return (
        <>
            <tr onClick={balanceClick} className="clickable" ref={balanceRef}>
                <td onClick={showVirtualAccountsClick}>{props.account.virtualAccounts && props.account.virtualAccounts.length > 0 && <FontAwesomeIcon icon={showVirtualAccounts ? "chevron-down" : "chevron-right"} />}</td>
                <td className="name">{props.account.name}</td>
                <td>{AccountType[props.account.accountType]}</td>
                <td className={classNames("number", numberClassName(props.account.currentBalance))}>
                    {!editingBalance && getBalanceString(balance)}
                    {editingBalance && <input type="number" value={balance} onChange={balanceChange} onKeyPress={keyPress} />}
                </td>
                {/*<td onClick={avBalanceClick} ref={avBalanceRef}> {!editingAvBalance && <span className={props.account.availableBalance < 0 ? " negative" : ""}>{getBalanceString(avBalance)}</span>}
                {editingAvBalance && <input type="number" value={avBalance} onChange={avBalanceChange} />}
            </td>*/}
            </tr>
            {props.account.virtualAccounts && props.account.virtualAccounts.length > 0 && showVirtualAccounts &&
                props.account.virtualAccounts.map(va =>
                    <VirtualAccountRow key={va.id} accountId={props.account.id} account={va} />
                )
            }
        </>
    );
}

ManualAccountRow.displayName = "ManualAccountRow";

const useComponentState = (props: AccountRowProps) => {

    const [editingBalance, setEditingBalance] = useState(false);

    const [balance, setBalance] = useState(props.account.currentBalance);

    const updateBalance = useUpdateBalance();

    const balanceRef = useRef(null);

    useClickAway(setEditingBalance, balanceRef, () => (editingBalance && props.account.currentBalance !== balance) && updateBalance.update(props.account.id, balance, balance));

    const balanceClick = (e: React.MouseEvent<HTMLTableRowElement>) => {
        setEditingBalance(true);
        e.preventDefault();
        e.stopPropagation();
    };

    const balanceChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        setBalance(parseFloat(e.currentTarget.value));
    }

    const keyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            updateBalance.update(props.account.id, balance, balance);
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

