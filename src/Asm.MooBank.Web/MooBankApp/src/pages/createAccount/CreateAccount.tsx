import React, { useState } from "react";
import { Form, Button, InputGroup, Col } from "react-bootstrap";
import { Account, AccountType, AccountController, ImportAccount } from "../../models";
import { toNameValue } from "../../extensions";
import { useNavigate } from "react-router-dom";

import { ImportSettings } from "./ImportSettings";
import { useAccountGroups, useCreateAccount } from "../../services";
import { emptyGuid } from "../../helpers";
import { Page } from "../../layouts";
import { FormGroup, FormRow } from "../../components";

export const CreateAccount: React.FC = () => {

    const navigate = useNavigate();

    const accountTypes = toNameValue(AccountType);
    const { data: accountGroups } = useAccountGroups();
    const createAccount = useCreateAccount();

    const accountControllers = toNameValue(AccountController);

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);
    const [accountType, setAccountType] = useState(AccountType.Transaction);
    const [accountController, setAccountController] = useState(AccountController.Manual);
    const [importerTypeId, setImporterTypeId] = useState(0);
    const [accountGroupId, setAccountGroupId] = useState("");

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const importAccount: ImportAccount | undefined = accountController === AccountController.Import ? { importerTypeId: importerTypeId } : undefined;

        const account: Account = {
            id: emptyGuid,
            name: name,
            description: description,
            availableBalance: balance,
            currentBalance: balance,
            accountType: accountType,
            controller: accountController,
            balanceUpdated: new Date(),
            accountGroupId: accountGroupId,
            virtualAccounts: [],
        };

        createAccount.create(account, importAccount);

        navigate("/");
    }

    return (
        <Page title="Create Account">
            <Page.Header title="Create Account" breadcrumbs={[["Manage Accounts", "/accounts"], ["Create Account", "/accounts/create"]]} />
            <Page.Content>
                <Form onSubmit={handleSubmit}>
                    <FormRow>
                        <FormGroup controlId="accountName" >
                            <Form.Label>Name</Form.Label>
                            <Form.Control type="text" required maxLength={50} value={name} onChange={(e: any) => setName(e.currentTarget.value)} />
                            <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="accountDescription" >
                            <Form.Label >Description</Form.Label>
                            <Form.Control type="text" as="textarea" required maxLength={255} value={description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                            <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="openingBalance" >
                            <Form.Label>Opening Balance</Form.Label>
                            <InputGroup>
                                <InputGroup.Text>$</InputGroup.Text>
                                <Form.Control type="number" required value={balance.toString()} onChange={(e: any) => setBalance(e.currentTarget.value)} />
                            </InputGroup>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="group">
                            <Form.Label>Group</Form.Label>
                            <Form.Control as="select" value={accountGroupId} onChange={(e: any) => setAccountGroupId(e.currentTarget.value)}>
                                {accountGroups?.map(a =>
                                    <option value={a.id} key={a.id}>{a.name}</option>
                                )}
                            </Form.Control>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="AccountType" >
                            <Form.Label>Type</Form.Label>
                            <Form.Select value={accountType.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setAccountType(parseInt(e.currentTarget.value))}>
                                {accountTypes.map(a =>
                                    <option value={a.value} key={a.value}>{a.name}</option>
                                )}
                            </Form.Select>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="AccountController">
                            <Form.Label>Controller</Form.Label>
                            <Form.Control as="select" value={accountController.toString()} onChange={(e: any) => setAccountController(parseInt(e.currentTarget.value))}>
                                {accountControllers.map(a =>
                                    <option value={a.value} key={a.value}>{a.name}</option>
                                )}
                            </Form.Control>
                        </FormGroup>
                    </FormRow>
                    <ImportSettings show={accountController === AccountController.Import} selectedId={importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                    <Button type="submit" variant="primary">Create</Button>
                </Form>
            </Page.Content>
        </Page>
    );
}

CreateAccount.displayName = "CreateAccount";