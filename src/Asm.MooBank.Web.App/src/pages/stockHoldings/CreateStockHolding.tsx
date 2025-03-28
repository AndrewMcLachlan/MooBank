import React, { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";
import { NewStockHolding, emptyStockHolding } from "../../models";

import { Page } from "@andrewmclachlan/mooapp";
import { useCreateStockHolding, useGroups } from "services";

export const CreateStockHolding: React.FC = () => {

    const navigate = useNavigate();

    const { data: groups } = useGroups();
    const createStockHolding = useCreateStockHolding();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [symbol, setSymbol] = useState("");
    const [quantity, setQuantity] = useState(0);
    const [price, setPrice] = useState(0);
    const [fees, setFees] = useState(0);
    const [groupId, setGroupId] = useState("");
    const [shareWithFamily, setShareWithFamily] = useState(false);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const stockHolding: NewStockHolding = {
            ...emptyStockHolding,
            name: name,
            symbol: symbol,
            quantity: quantity,
            price: price,
            fees: fees,
            description: description,
            controller: "Manual",
            currency: "AUD",
            currentBalanceLocalCurrency: 0,
            groupId: groupId === "" ? undefined : groupId,
            shareWithFamily: shareWithFamily,
            currentBalance: 0,
        };

        createStockHolding(stockHolding);

        navigate("/accounts");
    }

    return (
        <Page title="Create Shares" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Shares", route: "/stock/create" }]}>
            <Form className="section" onSubmit={handleSubmit}>
                <Form.Group controlId="accountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={name} onChange={(e: any) => setName(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="accountDescription" >
                    <Form.Label>Description</Form.Label>
                    <Form.Control type="text" as="textarea" required maxLength={255} value={description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="symbol" >
                    <Form.Label>Symbol</Form.Label>
                    <Form.Control type="text" required value={symbol} onChange={(e: any) => setSymbol(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a symbol</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="quantity" >
                    <Form.Label>Opening Quantity</Form.Label>
                    <InputGroup>
                        <Form.Control type="number" required value={quantity.toString()} onChange={(e: any) => setQuantity(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="price" >
                    <Form.Label>Purchase Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={price.toString()} onChange={(e: any) => setPrice(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="quantity" >
                    <Form.Label>Brokerage Fees</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={fees.toString()} onChange={(e: any) => setFees(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="group">
                    <Form.Label>Group</Form.Label>
                    <Form.Select value={groupId} onChange={(e: any) => setGroupId(e.currentTarget.value)}>
                        <option value="">Select a group...</option>
                        {groups?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <Form.Group controlId="ShareWithFamily">
                    <Form.Label>Visible to other family members</Form.Label>
                    <Form.Check checked={shareWithFamily} onChange={(e: any) => setShareWithFamily(e.currentTarget.checked)} />
                </Form.Group>
                <Button type="submit" variant="primary">Create</Button>
            </Form>
        </Page>
    );
}
