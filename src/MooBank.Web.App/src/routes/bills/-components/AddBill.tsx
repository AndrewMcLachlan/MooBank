import React, { useState, useEffect } from "react";
import { Form, Modal } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { format } from "date-fns";

import type { CreateBill } from "models/bills";
import { useCreateBill } from "../-hooks/useCreateBill";
import { useBillAccounts } from "../-hooks/useBillAccounts";
import { useChargeTypes } from "../-hooks/useChargeTypes";
import { BillForm, emptyPeriod } from "./BillForm";

export interface AddBillProps {
    accountId?: string;
    show: boolean;
    onHide: () => void;
}

export const AddBill: React.FC<AddBillProps> = ({ accountId, show, onHide }) => {

    const createBill = useCreateBill();
    const { data: accounts } = useBillAccounts();
    const { data: chargeTypes } = useChargeTypes();
    const [selectedAccountId, setSelectedAccountId] = useState<string>(accountId ?? "");

    useEffect(() => {
        if (accountId) {
            setSelectedAccountId(accountId);
        } else if (accounts && accounts.length > 0 && !selectedAccountId) {
            setSelectedAccountId(accounts[0].id);
        }
    }, [accountId, accounts, selectedAccountId]);

    const form = useForm<CreateBill>({
        defaultValues: {
            issueDate: format(new Date(), "yyyy-MM-dd"),
            costsIncludeGST: true,
            periods: [emptyPeriod()],
            discounts: [],
        }
    });

    const handleSubmit = async (data: CreateBill) => {
        if (!selectedAccountId) return;
        await createBill.mutateAsync(selectedAccountId, data);
        form.reset();
        onHide();
    };

    const handleClose = () => {
        form.reset();
        onHide();
    };

    return (
        <Modal show={show} onHide={handleClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Add Bill</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <BillForm
                    form={form}
                    chargeTypes={chargeTypes ?? []}
                    submitLabel="Create Bill"
                    pending={createBill.isPending}
                    onSubmit={handleSubmit}
                    onCancel={handleClose}
                    header={!accountId && (
                        <div className="form-row">
                            <Form.Group groupId="account">
                                <Form.Label>Account</Form.Label>
                                <Form.Select value={selectedAccountId} onChange={(e) => setSelectedAccountId(e.target.value)} required>
                                    {accounts?.map(account => (
                                        <option key={account.id} value={account.id}>{account.name}</option>
                                    ))}
                                </Form.Select>
                            </Form.Group>
                        </div>
                    )}
                />
            </Modal.Body>
        </Modal>
    );
};
