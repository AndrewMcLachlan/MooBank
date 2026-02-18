import React from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";


import { Page } from "@andrewmclachlan/moo-app";

import type { Family } from "api/types.gen";
import { emptyFamily } from "helpers/families";
import { useCreateFamily } from "services";
import { FamilyForm } from "./FamilyForm";

export const CreateFamily: React.FC = () => {

    const navigate = useNavigate();

    const createFamily = useCreateFamily();

    const handleSubmit = (data: Family) => {

        createFamily(data);

        navigate("/settings/families");
    }

    const form = useForm<Family>({ defaultValues: emptyFamily });

    return (
        <Page title="Create Family" breadcrumbs={[{ text: "Families", route: "/settings/families" }, { text: "Create Family", route: "/settings/families/add" }]}>
            <FamilyForm buttonText="Update" onSave={handleSubmit} />
        </Page>
    );
}

CreateFamily.displayName = "CreateFamily";
