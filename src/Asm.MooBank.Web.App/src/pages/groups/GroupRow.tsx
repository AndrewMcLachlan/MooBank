import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";
import { useNavigate } from "react-router";

import * as Models from "models";
import { Icon } from "@andrewmclachlan/moo-ds";

export const GroupRow: React.FC<GroupRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    return (
        <tr onClick={onRowClick} className="clickable">
            <td>
                <div className="name">{props.group.name}</div>
            </td>
            <td>
                {props.group.description}
            </td>
            <td className="row-action">
                {!!props.group.showTotal && <FontAwesomeIcon icon="check-circle" size="xl" />}
            </td>
        </tr>
    );
}

export interface GroupRowProps {
    group: Models.Group;
}

export const useAccountRowCommonState = (props: GroupRowProps) => {

    const navigate = useNavigate();

    const onRowClick = () => {
        navigate(`/groups/${props.group.id}/manage`);
    };

    return {
        onRowClick,
    };
}
