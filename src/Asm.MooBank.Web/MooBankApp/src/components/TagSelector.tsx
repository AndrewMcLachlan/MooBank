import { useTags } from "services"
import Select, { MultiValue } from "react-select"
import { TransactionTag } from "models";

export const TagSelector: React.FC<TagSelectorProps> = ({ value, multi, onChange }) => {

    const tags = useTags();

    const tag = typeof value === "number" ? tags.data?.find(t => t.id === value) : 
                Array.isArray(value) ? tags.data?.filter(t => value.includes(t.id)) :
                value as TransactionTag;

    const change = (v: TransactionTag | MultiValue<TransactionTag>) => {
        if (!onChange) return;

        if (multi) {
            onChange((v as MultiValue<TransactionTag>).map(t => t.id));
            return;
        }

        onChange((v as TransactionTag).id);
    }

    return (
        <Select options={tags.data ?? []} isMulti={multi} isClearable value={tag} getOptionLabel={t => t.name} getOptionValue={t => t.id.toString()} onChange={change} className="react-select" classNamePrefix="react-select" />
    )
}

TagSelector.defaultProps = {
    multi: false,
};

export interface TagSelectorProps {
    value?: number | TransactionTag | number[],
    multi?: boolean;
    onChange: (value: number | number[]) => void;
}
