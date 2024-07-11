import React from "react";
import Select, { Props } from "react-select";

import { Institution } from "models";
import { useInstitutions } from "services";

interface InstitutionSelectorProps extends Omit<Props<Institution>, "value" | "onChange"> {
    value: number;
    onChange: (value: number) => void;
}

export const InstitutionSelector: React.FC<InstitutionSelectorProps> = React.forwardRef<any, InstitutionSelectorProps>(({ value, onChange, ...props }, ref) => {

    const { data: institutions } = useInstitutions();

    const institution = value ? institutions?.find(c => c.id === value) : undefined;

    return (
        <Select onChange={(c: any) => onChange(c.code)} value={institution} options={institutions} getOptionValue={o => o.id.toString()} getOptionLabel={o => o.name} {...props} ref={ref} className="react-select" classNamePrefix="react-select" />
    );
});
