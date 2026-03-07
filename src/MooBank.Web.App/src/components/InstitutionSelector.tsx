import React from "react";

import type { AccountType } from "api/types.gen";
import { useInstitutionsByAccountType } from "hooks/useInstitutionsByAccountType";
import { FormComboBox } from "@andrewmclachlan/moo-ds";

interface InstitutionSelectorProps {
    accountType?: AccountType;
}

export const InstitutionSelector: React.FC<InstitutionSelectorProps> = (({ accountType }) => {

    const { data: institutions } = useInstitutionsByAccountType(accountType);

    return (
        <FormComboBox items={institutions ?? []} valueField={o => o?.id} labelField={o => o?.name} />
    );
});

InstitutionSelector.displayName = "InstitutionSelector";
