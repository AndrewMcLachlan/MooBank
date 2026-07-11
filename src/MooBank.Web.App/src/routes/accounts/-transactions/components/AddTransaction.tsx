import React from "react";
import { Button, Modal, Form } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { format } from "date-fns/format";

import { useAccount } from "components";
import type { CreateTransaction } from "models/transactions";
import { useUpdateBalance } from "hooks/useUpdateBalance";
import { useCreateTransaction } from "routes/accounts/-hooks/useCreateTransaction";
import { CurrencyInput } from "components";

type AddTransactionForm = Omit<CreateTransaction, "amount"> & {
    amount?: number | "";
    newBalance?: number | "";
};

// A number field is "filled" only when it holds a real value. Guards against "" (empty),
// undefined/null, and NaN (an empty native number input), while treating 0 as filled so a
// new balance of zero is accepted.
const isFilled = (value: unknown): boolean =>
    value !== undefined && value !== null && String(value).trim() !== "" && String(value) !== "NaN";

export const AddTransaction: React.FC<AddTransactionProps> = ({ show, onClose, onSave }) => {

    const account = useAccount();

    const addTransaction = useCreateTransaction();
    const updateBalance = useUpdateBalance();

    const isPending = addTransaction.isPending || updateBalance.isPending;

    const allowBalance = account?.controller === "Manual" || account?.controller === "Virtual";

    const form = useForm<AddTransactionForm>({
        defaultValues: {
            amount: "",
            newBalance: "",
            description: "",
            reference: "",
            transactionTime: format(new Date(), "yyyy-MM-dd"),
        },
    });

    const amountFilled = isFilled(form.watch("amount"));
    const newBalanceFilled = allowBalance && isFilled(form.watch("newBalance"));

    const handleSubmit = (values: AddTransactionForm) => {
        if (!account) return;

        if (newBalanceFilled) {
            updateBalance.mutateAsync(account.id, {
                amount: Number(values.newBalance),
                description: values.description,
                reference: values.reference,
                transactionTime: values.transactionTime,
            });
        } else {
            addTransaction.mutateAsync(account.id, {
                amount: Number(values.amount),
                description: values.description,
                reference: values.reference,
                transactionTime: values.transactionTime,
            });
        }

        onSave?.();
    };

    if (!account) return null;

    return (
        <Modal show={show} onHide={() => onClose()} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Add Transaction</Modal.Title>
            </Modal.Header>
            <Form form={form} onSubmit={handleSubmit} layout="horizontal">
                <Modal.Body>
                    <Form.Group groupId="amount">
                        <Form.Label>Amount</Form.Label>
                        <CurrencyInput disabled={newBalanceFilled} />
                    </Form.Group>
                    {allowBalance && (
                        <Form.Group groupId="newBalance">
                            <Form.Label>New Balance</Form.Label>
                            <CurrencyInput disabled={amountFilled} />
                        </Form.Group>
                    )}
                    <Form.Group groupId="transactionTime">
                        <Form.Label>Date</Form.Label>
                        <Form.Input type="date" required />
                    </Form.Group>
                    <Form.Group groupId="description">
                        <Form.Label>Description</Form.Label>
                        <Form.TextArea maxLength={255} />
                    </Form.Group>
                    <Form.Group groupId="reference">
                        <Form.Label>Reference</Form.Label>
                        <Form.Input type="text" maxLength={150} />
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={() => onClose()}>Close</Button>
                    <Button variant="primary" type="submit" disabled={isPending || (!amountFilled && !newBalanceFilled)}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}

export interface AddTransactionProps {
    show: boolean;
    onClose: () => void;
    onSave?: () => void;
}
