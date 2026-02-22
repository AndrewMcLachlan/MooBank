import React, { useEffect } from "react";
import { Button } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";

import { Form, SectionForm, FormComboBox } from "@andrewmclachlan/moo-ds";

import { CurrencySelector } from "components";
import { UtilityTypes } from "models/bills";
import { useCreateBillAccount } from "../-hooks/useCreateBillAccount";
import { useUser } from "hooks/useUser";

interface CreateBillAccountForm {
    name: string;
    description?: string;
    utilityType?: string;
    accountNumber?: string;
    currency: string;
    shareWithFamily: boolean;
}

export const BillAccountForm: React.FC = () => {

    const navigate = useNavigate();

    const createAccount = useCreateBillAccount();
    const { data: user } = useUser();

    const handleSubmit = async (data: CreateBillAccountForm) => {
        await createAccount.mutateAsync(data as any);
        navigate({ to: "/bills" });
    };

    const form = useForm<CreateBillAccountForm>({
        defaultValues: {
            currency: user?.currency ?? "AUD",
            shareWithFamily: true,
        }
    });

    useEffect(() => {
        form.reset({
            currency: user?.currency ?? "AUD",
            shareWithFamily: true,
        });
    }, [form, user]);

    return (
        <SectionForm form={form} onSubmit={handleSubmit}>
            <Form.Group groupId="name">
                <Form.Label>Name</Form.Label>
                <Form.Input type="text" required maxLength={50} />
            </Form.Group>
            <Form.Group groupId="description">
                <Form.Label>Description</Form.Label>
                <Form.TextArea maxLength={255} />
            </Form.Group>
            <Form.Group groupId="utilityType">
                <Form.Label>Utility Type</Form.Label>
                <FormComboBox placeholder="Select a utility type..." items={UtilityTypes} labelField={i => i} valueField={i => i} />
            </Form.Group>
            <Form.Group groupId="accountNumber">
                <Form.Label>Account Number</Form.Label>
                <Form.Input type="text" required maxLength={15} />
            </Form.Group>
            <Form.Group groupId="currency">
                <Form.Label>Currency</Form.Label>
                <CurrencySelector />
            </Form.Group>
            <Form.Group groupId="shareWithFamily" className="form-check">
                <Form.Check />
                <Form.Label className="form-check-label">Visible to other family members</Form.Label>
            </Form.Group>
            <Button type="submit" variant="primary" disabled={createAccount.isPending}>Create</Button>
        </SectionForm>
    );
};
