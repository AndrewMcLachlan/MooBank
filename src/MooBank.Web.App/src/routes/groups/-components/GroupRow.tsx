import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";
import { useNavigate } from "@tanstack/react-router";
import { useSortable } from "@dnd-kit/sortable";

import type { Group } from "api/types.gen";
import { colourRowProps } from "components";

export const GroupRow: React.FC<GroupRowProps> = (props) => {

    const { onRowClick } = useAccountRowCommonState(props);

    const { attributes, listeners, setNodeRef, setActivatorNodeRef, transform, transition, isDragging } = useSortable({ id: props.group.id });

    const { className, style } = colourRowProps(props.group.colour, "clickable");

    return (
        <tr
            ref={setNodeRef}
            onClick={onRowClick}
            className={isDragging ? `${className} dragging` : className}
            style={{
                ...style,
                // Only the vertical part of the drag is used: a table row that slides sideways out
                // of its columns looks broken, and there is nowhere for it to go.
                transform: transform ? `translate3d(0, ${transform.y}px, 0)` : undefined,
                transition,
            }}
        >
            <td className="drag-handle">
                <button
                    type="button"
                    ref={setActivatorNodeRef}
                    aria-label={`Reorder ${props.group.name}`}
                    // The row navigates on click; grabbing the handle is not that.
                    onClick={(e) => e.stopPropagation()}
                    {...attributes}
                    {...listeners}
                >
                    <FontAwesomeIcon icon="grip-vertical" />
                </button>
            </td>
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
