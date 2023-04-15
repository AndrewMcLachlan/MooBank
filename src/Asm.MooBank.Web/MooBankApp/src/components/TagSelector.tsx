import React from "react";
import { ComboBox } from "@andrewmclachlan/mooapp"
import { useTags } from "services"

export const TagSelector: React.FC<TagSelectorProps> = ({ value, onChange }) => {

    const tags = useTags();

    return (
        <ComboBox items={tags.data} valueField="id" textField="name" selectedItem={tags.data?.find(t => t.id === value)} onSelected={(v) => onChange && onChange(v.id)} />
    )
}

export interface TagSelectorProps {
    value?: number,
    onChange: (value: number) => void;
}