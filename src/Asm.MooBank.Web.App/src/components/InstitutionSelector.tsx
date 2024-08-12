import React from "react";
import Select, { Props } from "react-select";

import { AccountType, Institution } from "models";
import { useInstitutionsByAccountType } from "services";

interface InstitutionSelectorProps extends Omit<Props<Institution>, "value" | "onChange"> {
    value: number;
    onChange: (value: number) => void;
    accountType?: AccountType;
}

export const InstitutionSelector: React.FC<InstitutionSelectorProps> = React.forwardRef<any, InstitutionSelectorProps>(({ accountType, value, onChange, ...props }, ref) => {

    const { data: institutions } = useInstitutionsByAccountType(accountType);

    const institution = value ? institutions?.find(c => c.id === value) : undefined;

    return (
        <Select onChange={(c: any) => onChange(c.id)} value={institution} options={institutions} getOptionValue={o => o.id.toString()} getOptionLabel={o => o.name} {...props} ref={ref} className="react-select" classNamePrefix="react-select" />
    );
});

InstitutionSelector.displayName = "InstitutionSelector";
