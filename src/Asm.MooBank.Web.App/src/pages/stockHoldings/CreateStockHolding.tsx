import React, { useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";
import { NewStockHolding, emptyStockHolding } from "../../models";

import { Form, Page, SectionForm, useFormGroup } from "@andrewmclachlan/mooapp";
import { useCreateStockHolding, useGroups } from "services";
import { useForm, useFormContext } from "react-hook-form";
import { GroupSelector } from "components/GroupSelector";
import classNames from "classnames";

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

        createStockHolding.mutateAsync(stockHolding);

        navigate("/accounts");
    }

    const form = useForm<NewStockHolding>();

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
                        <Input2 type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="price" >
                    <Form.Label>Purchase Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Input2 type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="fees" >
                    <Form.Label>Brokerage Fees</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Input2 type="number" required />
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
                <Button type="submit" variant="primary" disabled={createStockHolding.isPending}>Save</Button>
            </SectionForm>
        </Page>
    );
}


export const Input2: React.FC<Input2Props> = ({ className, id, ...rest }) => {

    const group = useFormGroup();
    const { register } = useFormContext();
    id = id ?? group.groupId;
    const innerClass = rest.type === "checkbox" ? "form-check-input" : "form-control";

    return (
        <input id={id} className={classNames(innerClass, className)} {...rest} {...register(id, {
            setValueAs(value) {
                if (rest.type === "number") {
                    return value ? Number(value) : undefined;
                }
                return value;
            },
        })} />
    );
};


export interface Input2Props extends React.DetailedHTMLProps<React.InputHTMLAttributes<HTMLInputElement>, HTMLInputElement> {
    clearable?: boolean;
}
