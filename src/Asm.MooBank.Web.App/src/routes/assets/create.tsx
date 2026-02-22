import React from "react";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import type { CreateAsset as NewAsset } from "api/types.gen";
import { emptyAsset } from "models/assets";

import { Page } from "@andrewmclachlan/moo-app";
import { Form, SectionForm } from "@andrewmclachlan/moo-ds";
import { useCreateAsset } from "./-hooks/useCreateAsset";
import { useForm } from "react-hook-form";
import { GroupSelector } from "components/GroupSelector";
import { CurrencyInput } from "components";

export const Route = createFileRoute("/assets/create")({
    component: CreateAsset,
});

function CreateAsset() {

    const navigate = useNavigate();

    const createAsset = useCreateAsset();

    const handleSubmit = async (data: NewAsset) => {

        await createAsset.mutateAsync(data);

        navigate({ to: "/accounts" });
    }

    const form = useForm<NewAsset>();

    return (
        <Page title="Create Asset" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Asset", route: "/assets/create" }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name">
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description">
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="purchasePrice">
                    <Form.Label>Purchase Price</Form.Label>
                    <CurrencyInput required />
                </Form.Group>
                <Form.Group groupId="currentBalance">
                    <Form.Label>Current Value</Form.Label>
                    <CurrencyInput required />
                </Form.Group>
                <Form.Group groupId="groupId">
                    <Form.Label>Group</Form.Label>
                    <GroupSelector />
                </Form.Group>
                <Form.Group groupId="shareWithFamily" className="form-check">
                    <Form.Check />
                    <Form.Label className="form-check-label">Visible to other family members</Form.Label>
                </Form.Group>
                <Button type="submit" variant="primary" disabled={createAsset.isPending}>Save</Button>
            </SectionForm>
        </Page>
    );
}
