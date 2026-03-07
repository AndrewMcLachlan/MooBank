import React from "react";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";

import { Form, SectionForm, FormComboBox } from "@andrewmclachlan/moo-ds";

import { CurrencySelector, InstitutionSelector } from "components";
import { GroupSelector } from "components/GroupSelector";
import type { LogicalAccount, CreateAccount } from "api/types.gen";
import { AccountTypes } from "models/accounts";
import { Controllers } from "models/instruments";
import { useCreateAccount } from "../-hooks/useCreateAccount";
import { useUpdateAccount } from "../-hooks/useUpdateAccount";
import { useUser } from "hooks/useUser";
import { ImportSettings } from "../$id/manage/-components/ImportSettings";
import { CurrencyInput } from "components/CurrencyInput";
import { formatDate, formatISO } from "date-fns";

export const AccountForm: React.FC<{ account?: LogicalAccount }> = ({ account = null }) => {

    const navigate = useNavigate();

    const createAccount = useCreateAccount();
    const updateAccount = useUpdateAccount();
    const { data: user } = useUser();

    const isPending = createAccount.isPending || updateAccount.isPending;

    const handleSubmit = (data: CreateAccount) => {

        if (!account) {

            if (data.institutionId === undefined) {
                window.alert("Please select an institution");
                return;
            }

            createAccount.mutateAsync(data);
            navigate({ to: "/accounts" });
        } else {
            updateAccount.mutateAsync(data as any);
        }
    }

    const form = useForm<CreateAccount>({
        defaultValues: account ?? {
            currency: user?.currency,
        }
    });

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
            <Form.Group groupId="institutionId" show={!account}>
                <Form.Label>Institution</Form.Label>
                <InstitutionSelector accountType={accountType} /> {/* TODO: Required */}
            </Form.Group>
            <Form.Group groupId="currency">
                <Form.Label>Currency</Form.Label>
                <CurrencySelector />
            </Form.Group>
            <Form.Group groupId="balance" show={!account}>
                <Form.Label>Opening Balance</Form.Label>
                <CurrencyInput required defaultValue={0} />
            </Form.Group>
            <Form.Group groupId="openedDate" show={!account}>
                <Form.Label>Date Opened</Form.Label>
                <Form.Input type="date" required defaultValue={formatDate(new Date(), 'yyyy-MM-dd')} />
            </Form.Group>
            <Form.Group groupId="groupId">
                <Form.Label>Group</Form.Label>
                <GroupSelector />
            </Form.Group>
            <Form.Group groupId="controller">
                <Form.Label>Controller</Form.Label>
                <FormComboBox items={Controllers} labelField={i => i} valueField={i => i} />
            </Form.Group>
            <Form.Group groupId="importerTypeId" show={!account && controller === "Import"}>
                <Form.Label>Importer Type</Form.Label>
                <ImportSettings />
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
