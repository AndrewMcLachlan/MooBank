import { numberClassName } from "helpers";
import React, { useRef, useState } from "react";
import MD5 from "md5";

import { useClickAway } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classNames from "classnames";
import { useNavigate } from "react-router";
import { useUpdateBalance } from "services";
import { AccountRowProps } from "./AccountRow";
import { VirtualAccountRow } from "./VirtualAccountRow";
import { Amount } from "components/Amount";

export const ManualAccountRow: React.FC<AccountRowProps> = (props) => {

    const { balanceRef, editingBalance, balanceClick, onRowClick, balanceChange, balance, keyUp } = useComponentState(props);

    const [showVirtualAccounts, setShowVirtualAccounts] = useState<boolean>(localStorage.getItem(`account|${MD5(props.instrument.id)}`) === "true");

    const showVirtualAccountsClick = (e: React.MouseEvent<HTMLTableCellElement>) => {
        e.preventDefault();
        e.stopPropagation();
        setShowVirtualAccounts(!showVirtualAccounts);

        localStorage.setItem(`account|${MD5(props.instrument.id)}`, (!showVirtualAccounts).toString());
    }

    return (
        <>
            <tr onClick={onRowClick} className="clickable" ref={balanceRef}>
                <td className="d-none d-sm-table-cell" onClick={showVirtualAccountsClick}>{props.instrument.virtualInstruments && props.instrument.virtualInstruments.length > 0 && <FontAwesomeIcon icon={showVirtualAccounts ? "chevron-down" : "chevron-right"} />}</td>
                <td className="name">{props.instrument.name}</td>
                <td className="d-none d-sm-table-cell">{props.instrument.instrumentType}</td>
                <td className={classNames("amount", "number", numberClassName(props.instrument.currentBalance))} onClick={balanceClick}>
                    {!editingBalance && <Amount amount={balance} />}
                    {editingBalance && <input type="number" value={balance} onChange={balanceChange} onKeyUp={keyUp} />}
                </td>
            </tr>
            {props.instrument.virtualInstruments && props.instrument.virtualInstruments.length > 0 && showVirtualAccounts &&
                props.instrument.virtualInstruments.map(va =>
                    <VirtualAccountRow key={va.id} accountId={props.instrument.id} account={va} />
                )
            }
        </>
    );
}

ManualAccountRow.displayName = "ManualAccountRow";

const useComponentState = (props: AccountRowProps) => {

    const [editingBalance, setEditingBalance] = useState(false);

    const [balance, setBalance] = useState(props.instrument.currentBalance);

    const updateBalance = useUpdateBalance();

    const balanceRef = useRef(null);

    const navigate = useNavigate();

    useClickAway(setEditingBalance, balanceRef, () => (editingBalance && props.instrument.currentBalance !== balance) && doUpdateBalance());

    const balanceClick: React.MouseEventHandler<HTMLTableCellElement> = (e) => {
        setEditingBalance(true);
        e.preventDefault();
        e.stopPropagation();
    };

    const onRowClick = () => {
        navigate(`/accounts/${props.instrument.id}`);
    };

    const balanceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setBalance(parseFloat(e.currentTarget.value));
    }

    const keyUp = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            doUpdateBalance();
            setEditingBalance(false);
        }
    }

    const doUpdateBalance = () => {
        updateBalance.mutateAsync(props.instrument.id, { amount: balance, transactionTime: new Date().toISOString(), description: "Manual Balance Adjustment" });
    }

    return {
        balanceRef,

        editingBalance,

        balanceClick,
        onRowClick,

        balanceChange,

        balance,

        keyUp,
    };
}
