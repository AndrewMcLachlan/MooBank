import React, { useEffect, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";

import { Page, useIdParams } from "@andrewmclachlan/moo-app";

import type { Family } from "api/types.gen";
import { useFamily, useUpdateFamily } from "services";
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

    const form = useForm<Family>({ defaultValues: family });

    useEffect(() => {
        form.reset(family);
    }, [family, form]);

    return (
        <Page title="Create Family" breadcrumbs={[{ text: "Families", route: "/settings/families" }, { text: family?.name, route: `/settings/families/{${family?.id}}` }]}>
           <FamilyForm buttonText="Update" onSave={handleSubmit} family={family} />
        </Page>
    );
}
