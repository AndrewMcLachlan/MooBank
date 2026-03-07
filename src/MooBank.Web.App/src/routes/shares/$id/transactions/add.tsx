import { createFileRoute } from "@tanstack/react-router";
import { Button, Form, SectionForm } from "@andrewmclachlan/moo-ds";
import { type CreateStockTransaction, emptyStockTransaction } from "models/stocks";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";
import { useCreateStockTransaction } from "routes/shares/-hooks/useCreateStockTransaction";
import { StockHoldingPage } from "../../-components/StockHoldingPage";
import { useStockHolding } from "../../-components/StockHoldingProvider";

export const Route = createFileRoute("/shares/$id/transactions/add")({
    component: AddStockTransaction,
});

function AddStockTransaction() {

    const navigate = useNavigate();

    const stockHolding = useStockHolding();

    const addTransaction = useCreateStockTransaction();

    const form = useForm<CreateStockTransaction>({
        defaultValues: emptyStockTransaction,
    });

    const handleSubmit = (data: CreateStockTransaction) => {
        addTransaction(stockHolding.id, data);
        navigate({ to: `/shares/${stockHolding.id}/transactions` });
    };

    if (!stockHolding) return null;

    return (
        <StockHoldingPage title="Add Transaction" breadcrumbs={[{ text: "Add", route: `shares/${stockHolding.id}/transactions/add` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="quantity">
                    <Form.Label>Quantity</Form.Label>
                    <Form.Input type="number" required maxLength={10} />
                </Form.Group>
                <Form.Group groupId="price">
                    <Form.Label>Price</Form.Label>
                    <Form.Input type="number" required maxLength={10} />
                </Form.Group>
                <Form.Group groupId="fees">
                    <Form.Label>Fees</Form.Label>
                    <Form.Input type="number" required maxLength={10} />
                </Form.Group>
                <Form.Group groupId="date">
                    <Form.Label>Date</Form.Label>
                    <Form.Input type="date" required />
                </Form.Group>
                <Form.Group groupId="description">
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea maxLength={255} />
                </Form.Group>
                <Button type="submit" variant="primary">Add</Button>
            </SectionForm>
        </StockHoldingPage>
    );
}
