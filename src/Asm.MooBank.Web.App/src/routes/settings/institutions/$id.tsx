import { createFileRoute } from "@tanstack/react-router";
import { useInstitution } from "services";
import { SettingsPage } from "../-components/SettingsPage";
import { InstitutionForm } from "./-components/InstitutionForm";
import { useIdParams } from "@andrewmclachlan/moo-app";

export const Route = createFileRoute("/settings/institutions/$id")({
    component: ManageInstitution,
});

function ManageInstitution() {

    const id = useIdParams();

    const { data: institution } = useInstitution(Number(id));


    if (!institution) return null;

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }, { text: institution?.name, route: `/settings/institutions/${institution.id}` }]}>
            <InstitutionForm institution={institution} />
        </SettingsPage>
    );
}
