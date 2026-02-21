import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";
import { useNavigate } from "@tanstack/react-router";

import type { Group } from "api/types.gen";
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
    group: Group;
}

export const useAccountRowCommonState = (props: GroupRowProps) => {

    const navigate = useNavigate();

    const onRowClick = () => {
        navigate({ to: `/groups/${props.group.id}/manage` });
    };

    return {
        onRowClick,
    };
}
