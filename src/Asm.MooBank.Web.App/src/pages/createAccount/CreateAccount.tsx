import { Input, Page, Form, SectionForm, FormComboBox } from "@andrewmclachlan/mooapp";
import { useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router";

import { CurrencySelector, InstitutionSelector } from "components";
import { AccountType, AccountTypes, Controller, Controllers, CreateInstitutionAccount, Group } from "models";
import { useCreateAccount, useGroups } from "services";
import { ImportSettings } from "./ImportSettings";
import { GroupSelectorById as GroupSelector } from "components/GroupSelector";
import { useForm } from "react-hook-form";

export const CreateAccount: React.FC = () => {

    const navigate = useNavigate();

    const createAccount = useCreateAccount();


    const handleSubmit = (data: CreateInstitutionAccount) => {

        // TODO: Remove once proper ComboBox validation is supported
        if (data.institutionId === undefined) {
            window.alert("Please select an institution");
            return;
        }

        const account: CreateInstitutionAccount = { 
            ...data,
            importerTypeId: data.controller === "Import" ? data.importerTypeId : undefined,
        };

        createAccount(account);

        navigate("/accounts");
    }

    const form = useForm<CreateInstitutionAccount>();

    const accountType = form.watch("accountType");
    const controller = form.watch("controller");

    return (
        <Page title="Create Account" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Account", route: "/accounts/create" }]}>
            <SectionForm<CreateInstitutionAccount> form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="accountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input type="text" required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="accountDescription" >
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="AccountType" >
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
                <Form.Group groupId="openingBalance" >
                    <Form.Label>Opening Balance</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Input type="number" required />
                    </InputGroup>
                </Form.Group>
                <Form.Group groupId="openingDate">
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
                <Button type="submit" variant="primary">Create</Button>
            </SectionForm>
        </Page>
    );
}

CreateAccount.displayName = "CreateAccount";
