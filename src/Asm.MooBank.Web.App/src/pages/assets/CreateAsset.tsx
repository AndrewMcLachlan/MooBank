import React from "react";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "react-router";
import { NewAsset, emptyAsset } from "../../models";

import { Page } from "@andrewmclachlan/moo-app";
import { Form, SectionForm } from "@andrewmclachlan/moo-ds";
import { useCreateAsset } from "services";
import { useForm } from "react-hook-form";
import { GroupSelector } from "components/GroupSelector";
import { CurrencyInput } from "components";

export const CreateAsset: React.FC = () => {

    const navigate = useNavigate();

    const createAsset = useCreateAsset();

    const handleSubmit = async (data: NewAsset) => {

        await createAsset.mutateAsync(data);

        navigate("/accounts");
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
