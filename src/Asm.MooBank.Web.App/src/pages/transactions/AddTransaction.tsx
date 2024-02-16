import React, { useState } from "react";
import { Form, Button } from "react-bootstrap";
import * as Models from "models";
import { useCreateTransaction } from "services";
import { useNavigate } from "react-router-dom";
import { Section } from "@andrewmclachlan/mooapp";
import { AccountPage, useAccount } from "components";
import { useAccountRoute } from "hooks/useAccountRoute";

export const AddTransaction = () => {

    const navigate = useNavigate();

    const account = useAccount();
    const route = useAccountRoute();


    const [transaction, setTransaction] = useState<Models.CreateTransaction>(Models.emptyTransaction);

    const addTransaction = useCreateTransaction();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        addTransaction(account.id, transaction);

        navigate(`${route}/transactions`);
    }

    if (!account) return null;

    return (
        <AccountPage title="Add Transaction" breadcrumbs={[{ text: "Add", route: `${route}/transactions/add` }]}>
            {account &&
                <>
                    <Section>
                        <Form onSubmit={handleSubmit}>
                            <Form.Group controlId="Amount">
                                <Form.Label>Amount</Form.Label>
                                <Form.Control type="number" required maxLength={10} value={transaction.amount} onChange={(e: any) => setTransaction({ ...transaction, amount: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter an amount</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Date">
                                <Form.Label>Date</Form.Label>
                                <Form.Control type="date" required value={transaction.transactionTime} onChange={(e: any) => setTransaction({ ...transaction, transactionTime: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a date</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Description">
                                <Form.Label >Description</Form.Label>
                                <Form.Control type="text" as="textarea" maxLength={255} value={transaction.description} onChange={(e: any) => setTransaction({ ...transaction, description: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Reference">
                                <Form.Label>Reference</Form.Label>
                                <Form.Control type="text" maxLength={150} value={transaction.reference} onChange={(e: any) => setTransaction({ ...transaction, reference: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                            </Form.Group>
                            <Button type="submit" variant="primary">Add</Button>
                        </Form>
                    </Section>
                </>
            }
        </AccountPage>
    );
}
