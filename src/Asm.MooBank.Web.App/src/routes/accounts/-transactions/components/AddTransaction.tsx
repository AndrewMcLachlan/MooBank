import React from "react";
import { Button, Modal } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";

import { Form } from "@andrewmclachlan/moo-ds";

import { useAccount } from "components";
import type { Transaction } from "api/types.gen";
import { CreateTransaction, emptyTransaction } from "helpers/transactions";
import { useCreateTransaction, useUpdateBalance } from "services";
import { CurrencyInput } from "components";


export const AddTransaction: React.FC<AddTransactionProps> = ({ show, onClose, onSave, balanceUpdate }) => {

    const account = useAccount();

    const addTransaction = useCreateTransaction();
    const updateBalance = useUpdateBalance();

    const isPending = addTransaction.isPending || updateBalance.isPending;

    const handleSubmit = (transaction: CreateTransaction) => {

        if (balanceUpdate) {
            updateBalance.mutateAsync(account.id, transaction);
        } else {
            addTransaction.mutateAsync(account.id, transaction);
        }

        onSave?.();
    }

    const form = useForm<CreateTransaction>({ defaultValues: emptyTransaction });

    if (!account) return null;

    return (
        <Modal show={show} onHide={() => onClose()} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>{balanceUpdate ? "Balance Update" : "Add Transaction"}</Modal.Title>
            </Modal.Header>
            <Form form={form} onSubmit={handleSubmit} layout="horizontal">
                <Modal.Body>
                    <Form.Group groupId="amount">
                        <Form.Label>Amount</Form.Label>
                        <CurrencyInput />
                    </Form.Group>
                    <Form.Group groupId="transactionTime">
                        <Form.Label>Date</Form.Label>
                        <Form.Input type="date" required />
                    </Form.Group>
                    <Form.Group groupId="description">
                        <Form.Label >Description</Form.Label>
                        <Form.TextArea maxLength={255} />
                    </Form.Group>
                    <Form.Group groupId="reference">
                        <Form.Label>Reference</Form.Label>
                        <Form.Input type="text" maxLength={150} />
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={() => onClose()}>Close</Button>
                    <Button variant="primary" type="submit" disabled={isPending}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}

export interface AddTransactionProps {
    show: boolean;
    onClose: () => void;
    onSave?: () => void;
    balanceUpdate?: boolean;
}
