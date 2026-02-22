import { createFileRoute } from "@tanstack/react-router";
import { emptyInstitution } from "models/institutions";

import { SettingsPage } from "../-components/SettingsPage";
import { InstitutionForm } from "./-components/InstitutionForm";

export const Route = createFileRoute("/settings/institutions/add")({
    component: CreateInstitution,
});

function CreateInstitution() {

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }, { text: "Add", route: "/settings/institutions/add" }]}>
            <InstitutionForm institution={emptyInstitution} />
        </SettingsPage>
    );
}
