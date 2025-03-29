import React, { useEffect } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import {Form, SectionForm, FormComboBox } from "@andrewmclachlan/mooapp";

import { CurrencySelector, InstitutionSelector } from "components";
import { GroupSelector } from "components/GroupSelector";
import { AccountTypes, Controllers, CreateInstitutionAccount, InstitutionAccount } from "models";
import { useCreateAccount, useUpdateAccount, } from "services";
import { ImportSettings } from "./ImportSettings";

export const AccountForm: React.FC<{ account?: InstitutionAccount }> = ({ account = null }) => {

    const navigate = useNavigate();

    const createAccount = useCreateAccount();
    const updateAccount = useUpdateAccount();


    const handleSubmit = (data: CreateInstitutionAccount) => {

        if (data.institutionId === undefined) {
            window.alert("Please select an institution");
            return;
        }

        if (!account) {
            createAccount(data);
            navigate("/accounts");
        } else {
            updateAccount(data);
        }
    }

    const form = useForm<CreateInstitutionAccount>({ defaultValues: account });

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
            <Form.Group groupId="institutionId">
                <Form.Label>Institution</Form.Label>
                <InstitutionSelector accountType={accountType} /> {/* TODO: Required */}
            </Form.Group>
            <Form.Group groupId="currency">
                <Form.Label>Currency</Form.Label>
                <CurrencySelector />
            </Form.Group>
            <Form.Group groupId="openingBalance" show={!account}>
                <Form.Label>Opening Balance</Form.Label>
                <InputGroup>
                    <InputGroup.Text>$</InputGroup.Text>
                    <Form.Input type="number" required  />
                </InputGroup>
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
            <Form.Group groupId="importerTypeId" hidden={controller !== "Import"}>
                <Form.Label>Importer Type</Form.Label>
                <ImportSettings />
            </Form.Group>
            <Form.Group groupId="includeInBudget">
                <Form.Label>Include this account in the budget</Form.Label>
                <Form.Check />
            </Form.Group>
            <Form.Group groupId="shareWithFamily">
                <Form.Label>Visible to other family members</Form.Label>
                <Form.Check />
            </Form.Group>
            <Button type="submit" variant="primary">Save</Button>
        </SectionForm>
    );
};
