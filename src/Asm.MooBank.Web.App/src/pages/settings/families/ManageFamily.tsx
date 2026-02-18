import React, { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";


import { Page, useIdParams } from "@andrewmclachlan/moo-app";

import type { Family } from "api/types.gen";
import { useFamily, useUpdateFamily } from "services";
import { FamilyForm } from "./FamilyForm";

export const ManageFamily: React.FC = () => {

    const id = useIdParams();

    const { data: family } = useFamily(id);

    const navigate = useNavigate();

    const updateFamily = useUpdateFamily();

    const handleSubmit = (data: Family) => {

        updateFamily(data);

        navigate("/settings/families");
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

ManageFamily.displayName = "CreateFamily";
