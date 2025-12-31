import React, { useEffect } from "react";
import { Button } from "react-bootstrap";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { Form, SectionForm, FormComboBox } from "@andrewmclachlan/moo-ds";

import { CurrencySelector } from "components";
import { CreateBillAccount, UtilityTypes } from "models/bills";
import { useCreateBillAccount } from "services/BillService";
import { useUser } from "services";

export const BillAccountForm: React.FC = () => {

    const navigate = useNavigate();

    const createAccount = useCreateBillAccount();
    const { data: user } = useUser();

    const handleSubmit = async (data: CreateBillAccount) => {
        await createAccount.mutateAsync(data);
        navigate("/bills");
    };

    const form = useForm<CreateBillAccount>({
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
