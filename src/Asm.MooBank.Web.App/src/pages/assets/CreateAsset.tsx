import React, { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { NewAsset, emptyAsset } from "../../models";

import { Page } from "@andrewmclachlan/mooapp";
import { useGroups, useCreateAsset } from "services";

export const CreateAsset: React.FC = () => {

    const navigate = useNavigate();

    const { data: groups } = useGroups();
    const createAsset = useCreateAsset();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [purchasePrice, setPurchasePrice] = useState(0);
    const [currentValue, setCurrentValue] = useState(0);
    const [groupId, setGroupId] = useState("");
    const [shareWithFamily, setShareWithFamily] = useState(false);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const asset: NewAsset = {
            ...emptyAsset,
            name: name,
            purchasePrice: purchasePrice,
            description: description,
            controller: "Manual",
            currency: "AUD",
            currentBalanceLocalCurrency: 0,
            groupId: groupId === "" ? undefined : groupId,
            shareWithFamily: shareWithFamily,
            currentBalance: currentValue,
        };

        createAsset(asset);

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
                <Form.Group controlId="price" >
                    <Form.Label>Purchase Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" value={purchasePrice?.toString()} onChange={(e: any) => setPurchasePrice(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="value" >
                    <Form.Label>Current Value</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={currentValue.toString()} onChange={(e: any) => setCurrentValue(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="group">
                    <Form.Label>Group</Form.Label>
                    <Form.Control as="select" value={groupId} onChange={(e: any) => setGroupId(e.currentTarget.value)}>
                        <option value="">Select a group...</option>
                        {groups?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Control>
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
