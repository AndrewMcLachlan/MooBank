import React, { useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";
import { NewStockHolding, emptyStockHolding } from "../../models";

import { Form, Page, SectionForm } from "@andrewmclachlan/mooapp";
import { useCreateStockHolding, useGroups } from "services";
import { useForm } from "react-hook-form";
import { GroupSelector } from "components/GroupSelector";

export const CreateStockHolding: React.FC = () => {

    const navigate = useNavigate();

    const createStockHolding = useCreateStockHolding();


    const handleSubmit = (data: NewStockHolding) => {

        const stockHolding: NewStockHolding = {
            ...data,
            controller: "Manual",
            currency: "AUD",
            currentBalance: 0,
        };

        createStockHolding(stockHolding);

        navigate("/accounts");
    }

    const form = useForm({defaultValues: emptyStockHolding});

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
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Input type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="quantity" >
                    <Form.Label>Brokerage Fees</Form.Label>
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
                <Button type="submit" variant="primary">Create</Button>
            </SectionForm>
        </Page>
    );
}
