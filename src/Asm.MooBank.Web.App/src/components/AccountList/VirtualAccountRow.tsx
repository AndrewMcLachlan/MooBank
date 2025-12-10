import classNames from "classnames";
import React, { useEffect, useRef, useState } from "react";

import { numberClassName } from "helpers";
import { EditColumn, emptyGuid, useClickAway } from "@andrewmclachlan/moo-ds";

import { InstrumentId, VirtualInstrument } from "models";
import { useUpdateVirtualInstrumentBalance } from "services";
import { useNavigate } from "react-router";
import { Amount } from "components/Amount";

export const VirtualAccountRow: React.FC<VirtualAccountRowProps> = (props) => {
    const { balanceRef, editingBalance, balanceClick, balanceChange, balance, keyPress, onRowClick } = useComponentState(props);

    const clickable = props.account.id !== emptyGuid && props.account.controller === "Virtual";
    const canEditBalance = props.account.id !== emptyGuid && props.account.controller === "Manual";

    return (
        <tr onClick={clickable ? onRowClick : undefined} className={classNames("virtual", clickable && "clickable", "d-none", "d-sm-table-row")}>
            <td></td>
            <td className="name">{props.account.name}</td>
            <td className="d-none d-sm-table-cell">{props.account.controller === "Virtual" ? "Virtual" : "Reserved Sum"}</td>
            <td className={classNames("number", numberClassName(balance))} onClick={canEditBalance ? balanceClick : undefined}>
                {!editingBalance && <Amount amount={balance} negativeColour minus />}
                {editingBalance && <input autoFocus type="number" className="form-control" value={balance} onChange={balanceChange} onKeyUp={keyPress} ref={balanceRef} />}
            </td>
        </tr>
    );
}

interface VirtualAccountRowProps {
    account: VirtualInstrument;
    accountId: InstrumentId;
}

VirtualAccountRow.displayName = "VirtualAccountRow";

const useComponentState = (props: VirtualAccountRowProps) => {

    const navigate = useNavigate();

    const [editingBalance, setEditingBalance] = useState(false);
    const [balance, setBalance] = useState(props.account.currentBalance);

    const updateBalance = useUpdateVirtualInstrumentBalance();
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
