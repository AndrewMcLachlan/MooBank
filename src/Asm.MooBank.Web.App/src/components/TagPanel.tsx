import { TagPanel as MTagPanel, TagPanelProps as MTagPanelProps } from "@andrewmclachlan/mooapp";
import { Tag } from "models";
import { ElementType } from "react";

export const TagPanel: React.FC<TagPanelProps> = ({as = "div", allowCreate = false, readonly = false, alwaysShowEditPanel = false, ...props}) => (
    <MTagPanel<Tag> as={as} allowCreate={allowCreate} readonly={readonly} alwaysShowEditPanel={alwaysShowEditPanel} {...props} selectedItems={props.selectedItems} items={props.items} labelField={(t) => t.name} valueField={(t) => t.id?.toString()}  />
);

export interface TagPanelProps extends Omit<MTagPanelProps<Tag, ElementType>, "labelField" | "valueField"> {
}
