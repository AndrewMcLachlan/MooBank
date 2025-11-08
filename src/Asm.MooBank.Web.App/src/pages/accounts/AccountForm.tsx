import React, { useEffect } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { Form, SectionForm, FormComboBox } from "@andrewmclachlan/moo-ds";

import { CurrencySelector, InstitutionSelector } from "components";
import { GroupSelector } from "components/GroupSelector";
import { AccountTypes, Controllers, CreateLogicalAccount, LogicalAccount } from "models";
import { useCreateAccount, useUpdateAccount, } from "services";
import { ImportSettings } from "./ImportSettings";
import { CurrencyInput } from "components/CurrencyInput";

export const AccountForm: React.FC<{ account?: LogicalAccount }> = ({ account = null }) => {

    const navigate = useNavigate();

    const createAccount = useCreateAccount();
    const updateAccount = useUpdateAccount();

    const isPending = createAccount.isPending || updateAccount.isPending;

    const handleSubmit = (data: CreateLogicalAccount) => {

        if (data.institutionId === undefined) {
            window.alert("Please select an institution");
            return;
        }

        if (!account) {
            createAccount.mutateAsync(data);
            navigate("/accounts");
        } else {
            updateAccount.mutateAsync(data);
        }
    }

    const form = useForm<CreateLogicalAccount>({ defaultValues: account });

    useEffect(() => {
        form.reset(account);
    }, [account, form]);

    const accountType = form.watch("accountType");
    const controller = form.watch("controller");

    return (
        <SectionForm form={form} onSubmit={handleSubmit}>
            <Form.Group groupId="name" >
                <Form.Label>Name</Form.Label>
                <Form.Input type="text" required maxLength={50} />
            </Form.Group>
            <Form.Group groupId="description" >
                <Form.Label>Description</Form.Label>
                <Form.TextArea required maxLength={255} />
            </Form.Group>
            <Form.Group groupId="accountType" >
                <Form.Label>Type</Form.Label>
                <FormComboBox placeholder="Select an account type..." items={AccountTypes} labelField={i => i} valueField={i => i} />
            </Form.Group>
            <Form.Group groupId="currency">
                <Form.Label>Currency</Form.Label>
                <CurrencySelector />
            </Form.Group>
            <Form.Group groupId="balance" show={!account}>
                <Form.Label>Opening Balance</Form.Label>
                <CurrencyInput required />
            </Form.Group>
            <Form.Group groupId="openingDate" show={!account}>
                <Form.Label>Date Opened</Form.Label>
                <Form.Input type="date" required />
            </Form.Group>
            <Form.Group groupId="groupId">
                <Form.Label>Group</Form.Label>
                <GroupSelector />
            </Form.Group>
            <Form.Group groupId="controller">
                <Form.Label>Controller</Form.Label>
                <FormComboBox items={Controllers} labelField={i => i} valueField={i => i} />
            </Form.Group>
            <Form.Group groupId="includeInBudget" className="form-check">
                <Form.Check />
                <Form.Label className="form-check-label">Include this account in the budget</Form.Label>
            </Form.Group>
            <Form.Group groupId="shareWithFamily" className="form-check">
                <Form.Check />
                <Form.Label className="form-check-label">Visible to other family members</Form.Label>
            </Form.Group>
            <Button type="submit" variant="primary" disabled={isPending}>Save</Button>
        </SectionForm>
    );
};
