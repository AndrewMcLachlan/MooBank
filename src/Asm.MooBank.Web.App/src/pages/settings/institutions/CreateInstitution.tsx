import { emptyInstitution, Institution } from "models";
import { useNavigate } from "react-router";

import { useCreateInstitution } from "services";
import { SettingsPage } from "../SettingsPage";
import { InstitutionForm } from "./InstitutionForm";

export const CreateInstitution: React.FC = () => {

    const navigate = useNavigate();

    const createInstitution = useCreateInstitution();

    const handleSave = (institution: Institution) => {

        createInstitution(institution);

        navigate("/settings/institutions");
    }

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }, { text: "Add", route: "/settings/institutions/add" }]}>
            <InstitutionForm buttonText="Add" onSave={handleSave} institution={emptyInstitution} />
        </SettingsPage>
    );
}

CreateInstitution.displayName = "CreateInstitution";
