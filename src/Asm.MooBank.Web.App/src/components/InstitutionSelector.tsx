import React from "react";

import { AccountType, Institution } from "models";
import { useInstitutionsByAccountType } from "services";
import { ComboBox, ComboBoxProps } from "@andrewmclachlan/mooapp";

interface InstitutionSelectorProps extends Omit<ComboBoxProps<Institution>, "value" | "onChange"  | "items" | "valueField" | "labelField"> {
    value: number;
    onChange: (value: number) => void;
    accountType?: AccountType;
}

export const InstitutionSelector: React.FC<InstitutionSelectorProps> = (({ accountType, value, onChange, ...props }) => {

    const { data: institutions } = useInstitutionsByAccountType(accountType);

    const institution = value ? institutions?.find(c => c.id === value) : undefined;

    if (!institutions) return null;

    return (
        <ComboBox onChange={(c: Institution[]) => onChange(c.length > 0 ? c[0]?.id : undefined)} selectedItems={[institution]} items={institutions} valueField={o => o?.id.toString()} labelField={o => o?.name} {...props} />
    );
});

InstitutionSelector.displayName = "InstitutionSelector";
