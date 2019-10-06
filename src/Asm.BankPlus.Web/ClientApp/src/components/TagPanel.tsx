import React, { useState, useRef, ReactPropTypes, ElementType } from "react";
import { CloseBadge, ComboBox } from "components";
import { useClickAway } from "hooks/clickAway";

export const TagPanel: React.FC<TagPanelProps> = (props) => {

    const [editMode, setEditMode] = useState(false);
    const ref = useRef(null);
    useClickAway(setEditMode, ref);

    return (
        <props.as className="tag-panel" onClick={() => setEditMode(true)}>
            {props.selectedItems.map((i) => <CloseBadge onClose={() => props.onRemove && props.onRemove(i)} key={i[props.textField]} pill variant="light">{i[props.textField]}</CloseBadge>)}
            {editMode && <ComboBox ref={ref} items={props.allItems} textField={props.textField} onSelected={(props.onAdd)} onAdd={props.onCreate} allowAdd={props.allowCreate} />}
        </props.as>
    );
}

TagPanel.displayName = "TagPanel";

TagPanel.defaultProps = {
    textField: "name",
    as: "div",
    allowCreate: false,
}


export interface TagPanelProps {

    as: ElementType;
    allowCreate?: boolean;

    selectedItems: any[];
    allItems: any[];
    textField: string;

    search?: (search: string) => any[];

    onAdd?: (item: any) => void;
    onRemove?: (item: any) => void;
    onCreate?: (text: string) => void;
}