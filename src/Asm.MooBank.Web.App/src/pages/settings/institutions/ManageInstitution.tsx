import { Institution } from "models";
import { useNavigate } from "react-router";

import { useUpdateInstitution, useInstitution } from "services";
import { SettingsPage } from "../SettingsPage";
import { InstitutionForm } from "./InstitutionForm";
import { useIdParams } from "@andrewmclachlan/mooapp";

export const ManageInstitution: React.FC = () => {

    const id = useIdParams();

    const { data: institution } = useInstitution(Number(id));

    const navigate = useNavigate();

    const updateInstitution = useUpdateInstitution();

    const handleSave = (institution: Institution) => {

        updateInstitution(institution);

        navigate("/settings/institutions");
    }

    if (!institution) return null;

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }, { text: institution?.name, route: `/settings/institutions/${institution.id}` }]}>
            <InstitutionForm buttonText="Update" onSave={handleSave} institution={institution} />
        </SettingsPage>
    );
}

ManageInstitution.displayName = "ManageInstitution";
