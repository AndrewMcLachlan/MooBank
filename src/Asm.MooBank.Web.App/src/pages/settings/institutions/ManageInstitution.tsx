
import { useInstitution } from "services";
import { SettingsPage } from "../SettingsPage";
import { InstitutionForm } from "./InstitutionForm";
import { useIdParams } from "@andrewmclachlan/moo-app";

export const ManageInstitution: React.FC = () => {

    const id = useIdParams();

    const { data: institution } = useInstitution(Number(id));


    if (!institution) return null;

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }, { text: institution?.name, route: `/settings/institutions/${institution.id}` }]}>
            <InstitutionForm institution={institution} />
        </SettingsPage>
    );
}

ManageInstitution.displayName = "ManageInstitution";
