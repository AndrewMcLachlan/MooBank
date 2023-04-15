import React from "react";
import { useNavigate } from "react-router-dom";


import * as Models from "models";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const AccountGroupRow: React.FC<AccountGroupRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    return (
        <tr onClick={onRowClick} className="clickable">
            <td className="account">
                <div className="name">{props.accountGroup.name}</div>
            </td>
            <td>
                {props.accountGroup.description}
            </td>
            <td className="row-action">
                {props.accountGroup.showPosition && <FontAwesomeIcon icon="check-circle" size="lg" />}
            </td>
        </tr>
    );
}

AccountGroupRow.displayName = "AccountGroupRow";

export interface AccountGroupRowProps {
    accountGroup: Models.AccountGroup;
}

export const useAccountRowCommonState = (props: AccountGroupRowProps) => {

    var navigate = useNavigate();

    const onRowClick = () => {
        navigate(`/account-groups/${props.accountGroup.id}/manage`);
    };

    return {
        onRowClick,
    };
}
