import React from "react";
import { Button } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";

import { Form, SectionForm, FormComboBox } from "@andrewmclachlan/moo-ds";

import { CurrencySelector } from "components";
import type { Account, CreateBillAccount, UtilityType } from "api/types.gen";
import { UtilityTypes } from "models/bills";
import { useCreateBillAccount } from "../-hooks/useCreateBillAccount";
import { useUpdateBillAccount } from "../-hooks/useUpdateBillAccount";
import { useUser } from "hooks/useUser";

interface BillAccountFormValues {
    name: string;
    description?: string;
    utilityType?: UtilityType;
    accountNumber?: string;
    currency: string;
    shareWithFamily: boolean;
}

export const BillAccountForm: React.FC<BillAccountFormProps> = ({ account }) => {

    const navigate = useNavigate();

    const createAccount = useCreateBillAccount();
    const updateAccount = useUpdateBillAccount();
    const { data: user } = useUser();

    const isPending = createAccount.isPending || updateAccount.isPending;

    const handleSubmit = async (data: BillAccountFormValues) => {

        if (account) {
            await updateAccount.mutateAsync(account.id, {
                name: data.name,
                description: data.description?.trim() ? data.description : null,
                accountNumber: data.accountNumber,
                shareWithFamily: data.shareWithFamily,
            });
            navigate({ to: `/bills/accounts/${account.id}` });
            return;
        }

        await createAccount.mutateAsync(data as CreateBillAccount);
        navigate({ to: "/bills" });
    };

    const form = useForm<BillAccountFormValues>({
        values: account ? {
            name: account.name,
            description: account.description ?? "",
            utilityType: account.utilityType,
            accountNumber: account.accountNumber,
            currency: account.currency,
            shareWithFamily: account.shareWithFamily,
        } : {
            currency: user?.currency ?? "AUD",
            shareWithFamily: true,
        } as BillAccountFormValues,
        resetOptions: { keepDirtyValues: true },
    });

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
                {account ?
                    <Form.Input type="text" readOnly disabled /> :
                    <FormComboBox placeholder="Select a utility type..." items={UtilityTypes} labelField={i => i} valueField={i => i} />
                }
            </Form.Group>
            <Form.Group groupId="accountNumber">
                <Form.Label>Account Number</Form.Label>
                <Form.Input type="text" required maxLength={15} />
            </Form.Group>
            <Form.Group groupId="currency">
                <Form.Label>Currency</Form.Label>
                {account ?
                    <Form.Input type="text" readOnly disabled /> :
                    <CurrencySelector />
                }
            </Form.Group>
            <Form.Group groupId="shareWithFamily" className="form-check">
                <Form.Check />
                <Form.Label className="form-check-label">Visible to other family members</Form.Label>
            </Form.Group>
            <Button type="submit" variant="primary" disabled={isPending}>{account ? "Save" : "Create"}</Button>
        </SectionForm>
    );
};

export interface BillAccountFormProps {
    account?: Account;
}
