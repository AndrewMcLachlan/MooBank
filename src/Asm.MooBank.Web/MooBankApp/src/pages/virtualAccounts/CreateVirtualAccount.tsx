import React, { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Section, emptyGuid } from "@andrewmclachlan/mooapp";
import { Page } from "../../layouts";
import { VirtualAccount } from "../../models";
import { useAccount, useCreateVirtualAccount } from "../../services";
import { useAccountParams } from "../../hooks";
import { AccountPage } from "components";

export const CreateVirtualAccount = () => {

    const accountId = useAccountParams();
    const navigate = useNavigate();

    const parentAccount = useAccount(accountId);

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);

    const createVirtualAccount = useCreateVirtualAccount();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const account: VirtualAccount = {
            id: emptyGuid,
            name: name,
            description: description,
            balance: balance,
        };

        createVirtualAccount.create(accountId, account);

        navigate("/");
    }

    return (
        <AccountPage title="Create Virtual Account" breadcrumbs={[{text: "Create Virtual Account", route: `/accounts/${accountId}/manage/virtual/create`}]}>
            
            <Section>
                <Form onSubmit={handleSubmit}>
                    <Form.Group controlId="AccountName" >
                        <Form.Label>Name</Form.Label>
                        <Form.Control type="text" required maxLength={50} value={name} onChange={(e: any) => setName(e.currentTarget.value)} />
                        <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                    </Form.Group>
                    <Form.Group controlId="AccountDescription" >
                        <Form.Label >Description</Form.Label>
                        <Form.Control type="text" as="textarea" required maxLength={255} value={description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                        <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                    </Form.Group>
                    <Form.Group controlId="OpeningBalance" >
                        <Form.Label>Opening Balance</Form.Label>
                        <InputGroup>
                            <InputGroup.Text>$</InputGroup.Text>
                            <Form.Control type="number" required value={balance.toString()} onChange={(e: any) => setBalance(e.currentTarget.value)} />
                        </InputGroup>
                    </Form.Group>
                    <Button type="submit" variant="primary">Create</Button>
                </Form>
            </Section>
        </AccountPage>
    );
}

CreateVirtualAccount.displayName = "CreateVirtualAccount";