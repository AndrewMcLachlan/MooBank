import { emptyInstitution } from "models";

import { SettingsPage } from "../SettingsPage";
import { InstitutionForm } from "./InstitutionForm";

export const CreateInstitution: React.FC = () => {

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }, { text: "Add", route: "/settings/institutions/add" }]}>
            <InstitutionForm institution={emptyInstitution} />
        </SettingsPage>
    );
}

CreateInstitution.displayName = "CreateInstitution";
