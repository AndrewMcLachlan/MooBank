import { ComboBox, FormComboBox, RefProps, useFormGroup } from "@andrewmclachlan/mooapp";
import { Group } from "models";
import React from "react";
import { useGroups } from "services";

export const GroupSelector: React.FC<GroupSelectorProps> = ({ value, onChange, ref }) => {

    const { data: groups } = useGroups();

    return (
        <ComboBox selectedItems={value ? [value] : []} onChange={i => onChange(i?.[0])} placeholder="Select a group..." items={groups ?? []} labelField={i => i.name} valueField={i => i.id} ref={ref} />
    );
}

export interface GroupSelectorProps extends RefProps<HTMLInputElement> {
    value: Group | undefined;
    onChange: (group: Group | undefined) => void;
}

export const GroupSelectorById: React.FC<GroupSelectorByIdProps> = () => {

    const { data: groups } = useGroups();

    return (
        <FormComboBox placeholder="Select a group..." items={groups ?? []} labelField={i => i.name} valueField={i => i.id} />
    );
}

export interface GroupSelectorByIdProps {
}


