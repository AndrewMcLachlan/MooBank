import { TagPanel as MTagPanel, TagPanelProps as MTagPanelProps } from "@andrewmclachlan/moo-ds";
import { Tag } from "models";
import { ElementType } from "react";

export const TagPanel: React.FC<TagPanelProps> = ({ as = "div", allowCreate = false, readonly = false, alwaysShowEditPanel = false, ...props }) => {
    console.log("Items", props.selectedItems);
    return (
    <MTagPanel<Tag> as={as} creatable={allowCreate} readonly={readonly} alwaysShowEditPanel={alwaysShowEditPanel} {...props} selectedItems={props.selectedItems} items={props.items} labelField={(t) => t.name} valueField={(t) => t.id?.toString()} colourField={(t: Tag) => t.colour} />
);
};

export interface TagPanelProps extends Omit<MTagPanelProps<Tag, ElementType>, "labelField" | "valueField" | "colourField"> {
}
