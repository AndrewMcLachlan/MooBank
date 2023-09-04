import { TagPanel as MTagPanel, TagPanelProps as MTagPanelProps } from "@andrewmclachlan/mooapp";
import { Tag } from "models";
import { ElementType } from "react";


export const TagPanel = (props: TagPanelProps) => (
    <MTagPanel<Tag> {...props} selectedItems={props.selectedItems} items={props.items} labelField={(t) => t.name} valueField={(t) => t.id?.toString()}  />
);

TagPanel.displayName = "TransactionTagPanel";

TagPanel.defaultProps = {
    as: "div",
    allowCreate: false,
    readonly: false,
    alwaysShowEditPanel: false,
}

export interface TagPanelProps extends Omit<MTagPanelProps<Tag, ElementType>, "labelField" | "valueField"> {

}