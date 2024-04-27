import React, { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { NewAsset } from "../../models";

import { Page, emptyGuid } from "@andrewmclachlan/mooapp";
import { useAccountGroups, useCreateAsset } from "services";

export const CreateAsset: React.FC = () => {

    const navigate = useNavigate();

    const { data: accountGroups } = useAccountGroups();
    const createAsset = useCreateAsset();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    //const [quantity, setQuantity] = useState(0);
    const [purchasePrice, setPurchasePrice] = useState(0);
    const [currentValue, setCurrentValue] = useState(0);
    const [accountGroupId, setAccountGroupId] = useState("");
    const [shareWithFamily, setShareWithFamily] = useState(false);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const asset: NewAsset = {
            id: emptyGuid,
            name: name,
            purchasePrice: purchasePrice,
            description: description,
            currency: "AUD",
            currentBalanceLocalCurrency: 0,
            accountGroupId: accountGroupId === "" ? undefined : accountGroupId,
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
                {/* <Form.Group controlId="quantity" >
                    <Form.Label>Brokerage Fees</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={fees.toString()} onChange={(e: any) => setFees(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group> */}
                <Form.Group controlId="group">
                    <Form.Label>Group</Form.Label>
                    <Form.Control as="select" value={accountGroupId} onChange={(e: any) => setAccountGroupId(e.currentTarget.value)}>
                        <option value="">Select a group...</option>
                        {accountGroups?.map(a =>
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