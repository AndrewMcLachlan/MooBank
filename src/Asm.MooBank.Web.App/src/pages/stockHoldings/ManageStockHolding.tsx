import React, { useEffect, useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";
import { StockHolding } from "../../models";
import { StockHoldingPage } from "./StockHoldingPage";
import { useStockHolding } from "./StockHoldingProvider";

import { Form, SectionForm } from "@andrewmclachlan/mooapp";

import { useGroups, useUpdateStockHolding } from "services";
import { GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";

export const ManageStockHolding: React.FC = () => {

    const navigate = useNavigate();

    const { data: groups } = useGroups();
    const updateStockHolding = useUpdateStockHolding();

    const currentStockHolding = useStockHolding();

    useEffect(() => {
        setStockHolding(currentStockHolding);
    }, [currentStockHolding]);

    const [stockHolding, setStockHolding] = useState<StockHolding>();
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        updateStockHolding(stockHolding);

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
                <Form.Group groupId="price" >
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
                <Form.Group groupId="ShareWithFamily">
                    <Form.Label>Visible to other family members</Form.Label>
                    <Form.Check />
                </Form.Group>
                <Button type="submit" variant="primary">Update</Button>
            </SectionForm>
        </StockHoldingPage>
    );
}

ManageStockHolding.displayName = "ManageStockHolding";
