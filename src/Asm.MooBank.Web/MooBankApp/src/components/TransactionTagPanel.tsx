import { TagPanel, TagPanelProps } from "@andrewmclachlan/mooapp";
import { TransactionTag } from "models";
import { ElementType } from "react";


export const TransactionTagPanel = (props: TransactionTagPanelProps) => (
    <TagPanel<TransactionTag> {...props} selectedItems={props.selectedItems} items={props.items} labelField={(t) => t.name} valueField={(t) => t.id?.toString()}  />
);

TagPanel.displayName = "TransactionTagPanel";

TagPanel.defaultProps = {
    as: "div",
    allowCreate: false,
    readonly: false,
    alwaysShowEditPanel: false,
}

export interface TransactionTagPanelProps extends Omit<TagPanelProps<TransactionTag, ElementType>, "labelField" | "valueField"> {

}