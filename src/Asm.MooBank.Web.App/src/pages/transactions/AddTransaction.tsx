import React from "react";
import { Button, Modal } from "react-bootstrap";
import { useForm } from "react-hook-form";

import { Form, SectionForm, } from "@andrewmclachlan/mooapp";

import { useAccount } from "components";
import * as Models from "models";
import { useCreateTransaction, useUpdateBalance } from "services";


export const AddTransaction: React.FC<AddTransactionProps> = ({ show, onClose, onSave, balanceUpdate }) => {

    const account = useAccount();

    const addTransaction = useCreateTransaction();
    const updateBalance = useUpdateBalance();

    const isPending = addTransaction.isPending || updateBalance.isPending;

    const handleSubmit = (transaction: Models.Transaction) => {

        if (!!balanceUpdate) {
            updateBalance.mutateAsync(account.id, transaction);
        } else {
            addTransaction.mutateAsync(account.id, transaction);
        }

        onSave();
    }

    const form = useForm<Models.Transaction>({ defaultValues: Models.emptyTransaction });

    if (!account) return null;

    return (
        <Modal show={show} onHide={() => onClose()} size="lg" centered backdrop="static">
            <Modal.Header closeButton>
                <Modal.Title>{balanceUpdate ? "Balance Update" : "Add Transaction"}</Modal.Title>
            </Modal.Header>
            <Form form={form} onSubmit={handleSubmit} layout="horizontal">
                <Modal.Body>
                    <Form.Group groupId="amount">
                        <Form.Label>Amount</Form.Label>
                        <Form.Input type="number" required maxLength={10} />
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
