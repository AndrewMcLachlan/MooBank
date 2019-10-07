import React, { useState, useRef, ReactPropTypes, ElementType } from "react";
import { CloseBadge, ComboBox } from "components";
import { useClickAway } from "hooks/clickAway";

export const TagPanel: React.FC<TagPanelProps> = (props) => {

    const [editMode, setEditMode] = useState(false);
    const ref = useRef(null);
    useClickAway(setEditMode, ref);

    function event(item: any, event:(item: any) => void): void {
        setEditMode(false);
        if (event) event(item);
    }

    return (
        <props.as className="tag-panel" onClick={() => setEditMode(true)}>
            {props.selectedItems.map((i) => <CloseBadge onClose={() => event(i, props.onRemove)} key={i[props.textField]} pill variant="light">{i[props.textField]}</CloseBadge>)}
            {editMode && <ComboBox ref={ref} items={props.allItems} textField={props.textField} valueField={props.valueField} onSelected={(item) => event(item, props.onAdd)} onAdd={(item) => event(item, props.onCreate)} allowAdd={props.allowCreate} />}
        </props.as>
    );
}

TagPanel.displayName = "TagPanel";

TagPanel.defaultProps = {
    textField: "name",
    valueField: "id",
    as: "div",
    allowCreate: false,
}


export interface TagPanelProps {

    as: ElementType;
    allowCreate?: boolean;

    selectedItems: any[];
    allItems: any[];
    textField: string;
    valueField: string;

    search?: (search: string) => any[];

    onAdd?: (item: any) => void;
    onRemove?: (item: any) => void;
    onCreate?: (text: string) => void;
}