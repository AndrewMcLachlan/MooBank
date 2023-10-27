import { useTags } from "services"
import Select, { MultiValue } from "react-select"
import { Tag } from "models";

export const TagSelector: React.FC<TagSelectorProps> = ({ value, multi, onChange }) => {

    const tags = useTags();

    const tag = typeof value === "number" ? tags.data?.find(t => t.id === value) : 
                Array.isArray(value) ? tags.data?.filter(t => value.includes(t.id)) :
                value as Tag;

    const change = (v: Tag | MultiValue<Tag>) => {
        if (!onChange) return;

        if (multi) {
            onChange((v as MultiValue<Tag>).map(t => t.id));
            return;
        }

        onChange((v as Tag).id);
    }

    return (
        <Select options={tags.data ?? []} isMulti={multi} isClearable value={tag} getOptionLabel={t => t.name} getOptionValue={t => t.id.toString()} onChange={change} className="react-select" classNamePrefix="react-select" />
    )
}

TagSelector.defaultProps = {
    multi: false,
};

export interface TagSelectorProps {
    value?: number | Tag | number[],
    multi?: boolean;
    onChange: (value: number | number[]) => void;
}
