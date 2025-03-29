import { IconButton, Form, Section, useIdParams, SectionForm, FormComboBox } from "@andrewmclachlan/mooapp";
import { AccountPage, CurrencySelector, InstitutionSelector, useAccount } from "components";
import * as Models from "models";
import { AccountType, Controller } from "models";
import React, { useEffect, useState } from "react";
import { Button, Table } from "react-bootstrap";
import { useNavigate } from "react-router";
import { useGroups, useReprocessTransactions, useUpdateAccount, useVirtualAccounts } from "services";
import { ImportSettings } from "../createAccount/ImportSettings";
import { GroupSelectorById as GroupSelector } from "components/GroupSelector";
import { Controller as FormController, FormProvider, useForm } from "react-hook-form";

export const ManageAccount = () => {

    const reprocessTransactions = useReprocessTransactions();

    const navigate = useNavigate();

    const id = useIdParams();

    const existingAccount = useAccount();
    const { data: virtualAccounts } = useVirtualAccounts(existingAccount?.id ?? id);
    
    const updateAccount = useUpdateAccount();


    const handleSubmit = (data: Models.InstitutionAccount) => {
        updateAccount(data);
    }

    const getActions = (accountController: Controller) => {

        const actions = [<IconButton key="nva" onClick={() => navigate(`/accounts/${id}/manage/virtual/create`)} icon="plus">New Virtual Account</IconButton>];

        if (accountController === "Import") {
            actions.push(<IconButton key="rpt" onClick={() => reprocessTransactions.mutate({ instrumentId: id })} icon="arrows-rotate">Reprocess Transactions</IconButton>);
        }

        return actions;
    }

    const form = useForm<Models.InstitutionAccount>({ defaultValues: existingAccount });

    useEffect(() => {
        form.reset(existingAccount);
    }, [existingAccount, form]);

    const accountType = form.watch("accountType");
    const controller = form.watch("controller");

    return (
        <AccountPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/accounts/${existingAccount?.id}/manage` }]} actions={getActions(controller)}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name">
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description">
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea maxLength={255} />
                </Form.Group>
                <Form.Group groupId="currency">
                    <Form.Label>Currency</Form.Label>
                    <CurrencySelector />
                </Form.Group>
                <Form.Group groupId="institutionId">
                    <Form.Label>Institution</Form.Label>
                    <InstitutionSelector accountType={accountType} />
                </Form.Group>
                <Form.Group groupId="groupId">
                    <Form.Label>Group</Form.Label>
                    <GroupSelector />
                </Form.Group>
                <Form.Group groupId="accountType" >
                    <Form.Label>Type</Form.Label>
                    <FormComboBox placeholder="Select an account type..." items={Models.AccountTypes} labelField={i => i} valueField={i => i} />
                </Form.Group>
                <Form.Group groupId="controller">
                    <Form.Label>Controller</Form.Label>
                    <FormComboBox placeholder="Select a controller..." items={Models.Controllers} labelField={i => i} valueField={i => i} />
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
                <Button type="submit" variant="primary">Update</Button>
            </SectionForm>
            <Section title="Virtual Accounts">
                <Table striped hover>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        {virtualAccounts && virtualAccounts.map(a => (
                            <tr key={a.id} className="clickable" onClick={() => navigate(`/accounts/${existingAccount?.id}/manage/virtual/${a.id}`)}>
                                <td>{a.name}</td>
                                <td>{a.description}</td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            </Section>
        </AccountPage>
    );
}

export interface ManageAccountProps {
    account: Models.InstitutionAccount;
}
