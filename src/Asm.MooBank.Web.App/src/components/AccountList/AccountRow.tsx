import { MD5 } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classNames from "classnames";
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

import { getBalanceString, numberClassName } from "helpers";

import * as Models from "models";

import { VirtualAccountRow } from "./VirtualAccountRow";

export const AccountRow: React.FC<AccountRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    const [showVirtualAccounts, setShowVirtualAccounts] = useState<boolean>(localStorage.getItem(`account|${MD5(props.account.id)}`) === "true");

    const showVirtualAccountsClick = (e: React.MouseEvent<HTMLTableDataCellElement>) => {
        e.preventDefault();
        e.stopPropagation();
        setShowVirtualAccounts(!showVirtualAccounts);

        localStorage.setItem(`account|${MD5(props.account.id)}`, (!showVirtualAccounts).toString());
    }

    return (
        <>
            <tr onClick={onRowClick} className="clickable">
                <td className="d-none d-sm-table-cell" onClick={showVirtualAccountsClick}>{props.account.virtualAccounts && props.account.virtualAccounts.length > 0 && <FontAwesomeIcon icon={showVirtualAccounts ? "chevron-down" : "chevron-right"} />}</td>
                <td>{props.account.name}</td>
                <td className="d-none d-sm-table-cell">{props.account.accountType}</td>
                <td className={classNames("amount", "number", numberClassName(props.account.currentBalance))}>{getBalanceString(props.account.currentBalanceLocalCurrency)}</td>
            </tr>
            {showVirtualAccounts && props.account.virtualAccounts &&
                props.account.virtualAccounts.map(va => <VirtualAccountRow key={va.id} accountId={props.account.id} account={va} />)
            }
        </>
    );
}

AccountRow.displayName = "AccountRow";

export interface AccountRowProps {
    account: Models.InstitutionAccount;
}

export const useAccountRowCommonState = (props: AccountRowProps) => {

    const navigate = useNavigate();

    const onRowClick = () => {

        switch (props.account.accountType) {
            case "Stock Holding":
                navigate(`/stock/${props.account.id}`);
                break;
            default:
                navigate(`/accounts/${props.account.id}`);
                break;
        }
    };

    return {
        onRowClick,
    };
}

