import React, { useEffect, useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useMatch, useNavigate, useParams } from "react-router";
import { Section } from "@andrewmclachlan/mooapp";
import { VirtualAccount } from "../../models";
import { useUpdateVirtualAccount, useVirtualAccount } from "../../services";
import { AccountPage } from "components";
import { RecurringTransactions } from "./RecurringTransactions";

export const ManageVirtualAccount = () => {

    const navigate = useNavigate();

    const { id, virtualId } = useParams<{ id: string, virtualId: string }>();

    const { data: account } = useVirtualAccount(id, virtualId);

    const isDirect = useMatch("/accounts/:id/virtual/:virtualId/manage");

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);

    useEffect(() => {
        setName(account?.name ?? "");
        setDescription(account?.description ?? "");
        setBalance(account?.currentBalance ?? 0);
    }, [account]);

    const updateVirtualAccount = useUpdateVirtualAccount();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const updatedAccount: VirtualAccount = {
            ...account,
            name: name,
            description: description,
            currentBalance: balance,
        };

        updateVirtualAccount(id, updatedAccount);

        navigate(`/accounts/${id}/manage/`);
    }

    if (!account) return null;

    const breadcrumbs = isDirect ?
        [{ text: "Manage", route: `/accounts/${id}/virtual/${virtualId}/manage` }] :
        [{ text: "Manage", route: `/accounts/${id}/manage` }, { text: account.name, route: `/accounts/${id}/manage/virtual/${virtualId}` }]

    return (
        <AccountPage title="Manage Virtual Account" breadcrumbs={breadcrumbs}>

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
                        <Form.Label>Balance</Form.Label>
                        <InputGroup>
                            <InputGroup.Text>$</InputGroup.Text>
                            <Form.Control type="number" required value={balance.toString()} onChange={(e: any) => setBalance(e.currentTarget.value)} />
                        </InputGroup>
                    </Form.Group>
                    <Button type="submit" variant="primary">Save</Button>
                </Form>
            </Section>
            <Section title="Recurring Transactions">
                <RecurringTransactions account={account} />
            </Section>
        </AccountPage>
    );
}

ManageVirtualAccount.displayName = "MamageVirtualAccount";
