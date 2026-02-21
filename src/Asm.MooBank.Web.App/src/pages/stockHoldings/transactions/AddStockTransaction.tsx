import { Button, Form, SectionForm } from "@andrewmclachlan/moo-ds";
import type { CreateStockTransaction } from "helpers/stocks";
import { emptyStockTransaction } from "helpers/stocks";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";
import { useCreateStockTransaction } from "services";
import { StockHoldingPage } from "../StockHoldingPage";
import { useStockHolding } from "../StockHoldingProvider";

export const AddStockTransaction = () => {

    const navigate = useNavigate();

    const stockHolding = useStockHolding();

    const addTransaction = useCreateStockTransaction();

    const form = useForm<CreateStockTransaction>({
        defaultValues: emptyStockTransaction,
    });

    const handleSubmit = (data: CreateStockTransaction) => {
        addTransaction(stockHolding.id, data);
        navigate(`/shares/${stockHolding.id}/transactions`);
    };

    if (!stockHolding) return null;

    return (
        <StockHoldingPage title="Add Transaction" breadcrumbs={[{ text: "Add", route: `shares/${stockHolding.id}/transactions/add` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="quantity">
                    <Form.Label>Quantity</Form.Label>
                    <Form.Input type="number" required maxLength={10} />
                    {/* <Form.Feedback type="invalid">Please enter a quantity</Form.Feedback> */}
                </Form.Group>
                <Form.Group groupId="price">
                    <Form.Label>Price</Form.Label>
                    <Form.Input type="number" required maxLength={10} />
                    {/* <Form.Feedback type="invalid">Please enter a price</Form.Feedback> */}
                </Form.Group>
                <Form.Group groupId="fees">
                    <Form.Label>Fees</Form.Label>
                    <Form.Input type="number" required maxLength={10} />
                    {/* <Form.Feedback type="invalid">Please enter the fees</Form.Feedback> */}
                </Form.Group>
                <Form.Group groupId="date">
                    <Form.Label>Date</Form.Label>
                    <Form.Input type="date" required />
                    {/* <Form.Feedback type="invalid">Please enter a date</Form.Feedback> */}
                </Form.Group>
                <Form.Group groupId="description">
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea maxLength={255} />
                    {/* <Form.Feedback type="invalid">Please enter a description</Form.Feedback> */}
                </Form.Group>
                <Button type="submit" variant="primary">Add</Button>
            </SectionForm>
        </StockHoldingPage>
    );
};
