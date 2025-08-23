import React from "react";

import { FormComboBox } from "@andrewmclachlan/moo-ds";
import { useImporterTypes } from "services";

export const ImportSettings: React.FC = () => {

    const { data: importerTypes } = useImporterTypes();

    return (
        <FormComboBox items={importerTypes ?? []} valueField={o => o?.id} labelField={o => o?.name} />
    );
}

ImportSettings.displayName = "AccountControllerSettings";
