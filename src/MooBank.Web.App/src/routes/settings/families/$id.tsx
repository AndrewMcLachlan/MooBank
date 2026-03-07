import { createFileRoute } from "@tanstack/react-router";
import { useNavigate } from "@tanstack/react-router";

import { Page, useIdParams } from "@andrewmclachlan/moo-app";

import type { Family } from "api/types.gen";
import { useFamily } from "./-hooks/useFamily";
import { useUpdateFamily } from "./-hooks/useUpdateFamily";
import { FamilyForm } from "./-components/FamilyForm";

export const Route = createFileRoute("/settings/families/$id")({
    component: ManageFamily,
});

function ManageFamily() {

    const id = useIdParams();

    const { data: family } = useFamily(id);

    const navigate = useNavigate();

    const updateFamily = useUpdateFamily();

    const handleSubmit = (data: Family) => {

        updateFamily(data);

        navigate({ to: "/settings/families" });
    }

    return (
        <Page title="Create Family" breadcrumbs={[{ text: "Families", route: "/settings/families" }, { text: family?.name, route: `/settings/families/{${family?.id}}` }]}>
           <FamilyForm key={family?.id} buttonText="Update" onSave={handleSubmit} family={family} />
        </Page>
    );
}
