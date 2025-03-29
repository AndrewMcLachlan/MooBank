import React, { useEffect, useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";

import { Form, SectionForm } from "@andrewmclachlan/mooapp";

import { Asset } from "../../models";
import { AssetPage } from "./AssetPage";
import { useAsset } from "./AssetProvider";

import { useUpdateAsset } from "services";
import { GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";

export const ManageAsset: React.FC = () => {

    const navigate = useNavigate();

    const updateAsset = useUpdateAsset();

    const asset = useAsset();

    const handleSubmit = (data: Asset) => {

        updateAsset(data);

        navigate("/accounts");
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
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Input type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="currentBalance" >
                    <Form.Label>Current Value</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Input type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="groupId">
                    <Form.Label>Group</Form.Label>
                    <GroupSelector />
                </Form.Group>
                <Form.Group groupId="shareWithFamily">
                    <Form.Label>Visible to other family members</Form.Label>
                    <Form.Check />
                </Form.Group>
                <Button type="submit" variant="primary">Update</Button>
            </SectionForm>
        </AssetPage>
    );
}

ManageAsset.displayName = "ManageAsset";
