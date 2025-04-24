import MD5 from "md5";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classNames from "classnames";
import React, { useState } from "react";
import { useNavigate } from "react-router";

import { getBalanceString, numberClassName } from "helpers";

import * as Models from "models";

import { VirtualAccountRow } from "./VirtualAccountRow";
import { Amount } from "components/Amount";

export const AccountRow: React.FC<AccountRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    const [showVirtualAccounts, setShowVirtualAccounts] = useState<boolean>(localStorage.getItem(`account|${MD5(props.instrument.id)}`) === "true");

    const showVirtualAccountsClick = (e: React.MouseEvent<HTMLTableCellElement>) => {
        e.preventDefault();
        e.stopPropagation();
        setShowVirtualAccounts(!showVirtualAccounts);

        localStorage.setItem(`account|${MD5(props.instrument.id)}`, (!showVirtualAccounts).toString());
    }

    return (
        <>
            <tr onClick={onRowClick} className="clickable">
                <td className="d-none d-sm-table-cell" onClick={showVirtualAccountsClick}>{props.instrument.virtualInstruments && props.instrument.virtualInstruments.length > 0 && <FontAwesomeIcon icon={showVirtualAccounts ? "chevron-down" : "chevron-right"} />}</td>
                <td>{props.instrument.name}</td>
                <td className="d-none d-sm-table-cell">{props.instrument.instrumentType}</td>
                <td className={classNames("number", numberClassName(props.instrument.currentBalance))}><Amount amount={props.instrument.currentBalanceLocalCurrency} creditdebit/></td>
            </tr>
            {showVirtualAccounts && props.instrument.virtualInstruments &&
                props.instrument.virtualInstruments.map(va => <VirtualAccountRow key={va.id} accountId={props.instrument.id} account={va} />)
            }
        </>
    );
}

AccountRow.displayName = "AccountRow";

export interface AccountRowProps {
    instrument: Models.Instrument;
}

export const useAccountRowCommonState = (props: AccountRowProps) => {

    const navigate = useNavigate();

    const onRowClick = () => {

        switch (props.instrument.instrumentType) {
            case "Asset":
                navigate(`/assets/${props.instrument.id}`);
                break;
            case "Shares":
                navigate(`/shares/${props.instrument.id}`);
                break;
            default:
                navigate(`/accounts/${props.instrument.id}`);
                break;
        }
    };

    return {
        onRowClick,
    };
}
