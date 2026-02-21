import React from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "@tanstack/react-router";

import { Page } from "@andrewmclachlan/moo-app";
import { Form, SectionForm } from "@andrewmclachlan/moo-ds";

import { CurrencyInput } from "components";
import { GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";
import { useCreateStockHolding } from "./-hooks/useCreateStockHolding";
import type { CreateStock } from "api/types.gen";

export const Route = createFileRoute("/shares/create")({
    component: CreateStockHolding,
});

function CreateStockHolding() {

    const navigate = useNavigate();

    const createStockHolding = useCreateStockHolding();

    const handleSubmit = (data: CreateStock) => {

        const stockHolding = {
            ...data,
            controller: "Manual",
            currency: "AUD",
            currentBalance: 0,
        } as CreateStock;

        createStockHolding.mutateAsync(stockHolding);

        navigate({ to: "/accounts" });
    }

    const form = useForm<CreateStock>();

    return (
        <Page title="Create Shares" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Shares", route: "/stock/create" }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description" >
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="symbol" >
                    <Form.Label>Symbol</Form.Label>
                    <Form.Input required />
                </Form.Group>
                <Form.Group groupId="quantity" >
                    <Form.Label>Opening Quantity</Form.Label>
                    <InputGroup>
                        <Form.Input type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="price" >
                    <Form.Label>Purchase Price</Form.Label>
                    <CurrencyInput required />
                </Form.Group>
                <Form.Group groupId="fees" >
                    <Form.Label>Brokerage Fees</Form.Label>
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
                <Button type="submit" variant="primary" disabled={createStockHolding.isPending}>Save</Button>
            </SectionForm>
        </Page>
    );
}
