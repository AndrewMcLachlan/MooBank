import React, { useEffect } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";
import { StockHolding } from "../../models";
import { StockHoldingPage } from "./StockHoldingPage";
import { useStockHolding } from "./StockHoldingProvider";

import { Form, SectionForm } from "@andrewmclachlan/moo-ds";

import { useUpdateStockHolding } from "services";
import { GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";

export const ManageStockHolding: React.FC = () => {

    const navigate = useNavigate();

    const updateStockHolding = useUpdateStockHolding();

    const stockHolding = useStockHolding();

    const handleSubmit = (data: StockHolding) => {

        updateStockHolding.mutateAsync(data);

        navigate("/accounts");
    }

    const form = useForm({ defaultValues: stockHolding });

    useEffect(() => {
        form.reset(stockHolding);
    }, [stockHolding, form]);

    if (!stockHolding) return null;

    return (
        <StockHoldingPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/shares/${stockHolding.id}/manage` }]}>
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
                    <Form.Input required maxLength={3} readOnly />
                </Form.Group>
                <Form.Group groupId="currentPrice" >
                    <Form.Label>Current Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Input type="number" required />
                    </InputGroup>
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

ManageStockHolding.displayName = "ManageStockHolding";
