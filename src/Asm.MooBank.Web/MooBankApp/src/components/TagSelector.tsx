import { useTags } from "services"
import Select from "react-select"
import { TransactionTag } from "models";

export const TagSelector: React.FC<TagSelectorProps> = ({ value, onChange }) => {

    const tags = useTags();

    const tag = typeof value === "number" ? tags.data?.find(t => t.id === value) : value as TransactionTag;

    return (
        <Select options={tags.data ?? []} isClearable value={tag} getOptionLabel={t => t.name} getOptionValue={t => t.id.toString()} onChange={(v) => onChange && onChange(v?.id ?? null)} className="react-select" classNamePrefix="react-select" />
    )
}

export interface TagSelectorProps {
    value?: number | TransactionTag,
    onChange: (value: number) => void;
}

//<ComboBox items={tags.data} valueField="id" textField="name" selectedItem={tags.data?.find(t => t.id === value)} onSelected={(v) => onChange && onChange(v.id)} />