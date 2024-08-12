import { Section } from "@andrewmclachlan/mooapp";
import * as Models from "models";
import React, { useState } from "react";
import { Button, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useCreateStockTransaction } from "services";
import { StockHoldingPage } from "../StockHoldingPage";
import { useStockHolding } from "../StockHoldingProvider";

export const AddStockTransaction = () => {

    const navigate = useNavigate();

    const stockHolding = useStockHolding();

    const [transaction, setTransaction] = useState<Models.CreateStockTransaction>(Models.emptyStockTransaction);

    const addTransaction = useCreateStockTransaction();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        addTransaction(stockHolding.id, transaction);

        navigate(`/shares/${stockHolding.id}/transactions`);
    }

    if (!stockHolding) return null;

    return (
        <StockHoldingPage title="Add Transaction" breadcrumbs={[{ text: "Add", route: `shares/${stockHolding.id}/transactions/add` }]}>
            {stockHolding &&
                <>
                    <Section>
                        <Form onSubmit={handleSubmit}>
                            <Form.Group controlId="Quantity">
                                <Form.Label>Quantity</Form.Label>
                                <Form.Control type="number" required maxLength={10} value={transaction.quantity} onChange={(e: any) => setTransaction({ ...transaction, quantity: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a quantity</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Price">
                                <Form.Label>Price</Form.Label>
                                <Form.Control type="number" required maxLength={10} value={transaction.price} onChange={(e: any) => setTransaction({ ...transaction, price: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a price</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Fees">
                                <Form.Label>Fees</Form.Label>
                                <Form.Control type="number" required maxLength={10} value={transaction.fees} onChange={(e: any) => setTransaction({ ...transaction, fees: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter the fees</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Date">
                                <Form.Label>Date</Form.Label>
                                <Form.Control type="date" required value={transaction.date} onChange={(e: any) => setTransaction({ ...transaction, date: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a date</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="Description">
                                <Form.Label >Description</Form.Label>
                                <Form.Control type="text" as="textarea" maxLength={255} value={transaction.description} onChange={(e: any) => setTransaction({ ...transaction, description: e.currentTarget.value })} />
                                <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                            </Form.Group>
                            <Button type="submit" variant="primary">Add</Button>
                        </Form>
                    </Section>
                </>
            }
        </StockHoldingPage>
    );
}
