import { useTags } from "services"
import { ComboBox } from "@andrewmclachlan/mooapp";
import { Tag } from "models";

export const TagSelector: React.FC<TagSelectorProps> = ({ value, multiSelect = false, onChange }) => {

    const tags = useTags();

    const tag = typeof value === "number" ? [tags.data?.find(t => t.id === value)] :
        Array.isArray(value) ? tags.data?.filter(t => value.includes(t.id)) :
            [value];

    const change = (v: Tag[]) => {
        console.debug("TagSelector.change", v);
        onChange?.(v.map(t => t.id));
    }

    return (
        <ComboBox items={tags.data ?? []} selectedItems={tag} labelField={tag => tag?.name} valueField={tag => tag?.id} onChange={change} multiSelect={multiSelect} clearable />
    )
}

export interface TagSelectorProps {
    value?: number | Tag | number[],
    multiSelect?: boolean;
    onChange: (value: number | number[]) => void;
}
