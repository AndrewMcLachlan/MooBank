import React, { useEffect } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "@tanstack/react-router";
import type { StockHolding } from "api/types.gen";
import { StockHoldingPage } from "../-components/StockHoldingPage";
import { useStockHolding } from "../-components/StockHoldingProvider";

import { Form, SectionForm } from "@andrewmclachlan/moo-ds";

import { useUpdateStockHolding } from "services";
import { GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";
import { CurrencyInput } from "components";

export const Route = createFileRoute("/shares/$id/manage")({
    component: ManageStockHolding,
});

function ManageStockHolding() {

    const navigate = useNavigate();

    const updateStockHolding = useUpdateStockHolding();

    const stockHolding = useStockHolding();

    const handleSubmit = (data: StockHolding) => {

        updateStockHolding.mutateAsync(data);

        navigate({ to: "/accounts" });
    }

    const form = useForm({ defaultValues: stockHolding });

    useEffect(() => {
        form.reset(stockHolding);
    }, [stockHolding, form]);

    if (!stockHolding) return null;

    return (
        <StockHoldingPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/shares/${stockHolding.id}/manage` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name">
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description">
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="symbol">
                    <Form.Label>Symbol</Form.Label>
                    <Form.Input required maxLength={3} readOnly />
                </Form.Group>
                <Form.Group groupId="currentPrice">
                    <Form.Label>Current Price</Form.Label>
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
                <Button type="submit" variant="primary" disabled={updateStockHolding.isPending}>Save</Button>
            </SectionForm>
        </StockHoldingPage>
    );
}
