import React, { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { Redirect, useParams } from "react-router";
import { PageHeader } from "../../components";
import { emptyGuid } from "../../helpers";
import { usePageTitle } from "../../hooks";
import { VirtualAccount } from "../../models";
import { useAccount, useCreateVirtualAccount } from "../../services";

export const CreateVirtualAccount = () => {

    usePageTitle("Create Virtual Account");

    const { accountId } = useParams<any>();

    const parentAccount = useAccount(accountId);

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);
    const [submitted, setSubmitted] = useState(false);

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

        setSubmitted(true);
    }

    if (submitted) {
        return <Redirect to="/" />;
    }

    return (
        <>
            <PageHeader title="Create Virtual Account" breadcrumbs={[["Manage Accounts", "/accounts"], [parentAccount?.data?.name, `/accounts/${accountId}/manage`], ["Create Virtual Account", `/accounts/${accountId}/manage/virtual/create`]]} />
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
                        <InputGroup.Prepend>$</InputGroup.Prepend>
                        <Form.Control type="number" required value={balance.toString()} onChange={(e: any) => setBalance(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Button type="submit" variant="primary">Create</Button>
            </Form>
        </>
    );
}

CreateVirtualAccount.displayName = "CreateVirtualAccount";