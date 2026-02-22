import React, { useEffect, useState } from "react";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { createFileRoute, useNavigate } from "@tanstack/react-router";

import { Form, SectionForm } from "@andrewmclachlan/moo-ds";

import type { Asset } from "api/types.gen";
import { AssetPage } from "../-components/AssetPage";
import { useAsset } from "../-components/AssetProvider";

import { useUpdateAsset } from "../-hooks/useUpdateAsset";
import { GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";
import { CurrencyInput } from "components";

export const Route = createFileRoute("/assets/$id/manage")({
    component: ManageAsset,
});

function ManageAsset() {

    const navigate = useNavigate();

    const updateAsset = useUpdateAsset();

    const asset = useAsset();

    const handleSubmit = async (data: Asset) => {

        await updateAsset.mutateAsync(data);

        navigate({ to: "/accounts" });
    }

    const form = useForm({ defaultValues: asset });

    useEffect(() => {
        form.reset(asset);
    }, [asset, form]);

    return (
        <AssetPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/assets/${asset?.id}/manage` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description" >
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="purchasePrice" >
                    <Form.Label>Purchase Price</Form.Label>
                    <CurrencyInput required />
                </Form.Group>
                <Form.Group groupId="currentBalance" >
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
                <Button type="submit" variant="primary" disabled={updateAsset.isPending}>Save</Button>
            </SectionForm>
        </AssetPage>
    );
}
