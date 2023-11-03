import React, { useState } from "react";
import { Form, Button, InputGroup, Col } from "react-bootstrap";
import { Account, AccountTypes, AccountControllers, ImportAccount, AccountType, AccountController } from "../../models";
import { toNameValue } from "../../extensions";
import { useNavigate } from "react-router-dom";

import { ImportSettings } from "./ImportSettings";
import { useAccountGroups, useCreateAccount, useInstitutions } from "services";
import { emptyGuid, Page } from "@andrewmclachlan/mooapp";

export const CreateAccount: React.FC = () => {

    const navigate = useNavigate();

    const { data: accountGroups } = useAccountGroups();
    const { data: institutions } = useInstitutions();
    const createAccount = useCreateAccount();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);
    const [accountType, setAccountType] = useState<AccountType>("Transaction");
    const [accountController, setAccountController] = useState<AccountController>("Manual");
    const [importerTypeId, setImporterTypeId] = useState(0);
    const [accountGroupId, setAccountGroupId] = useState("");
    const [shareWithFamily, setShareWithFamily] = useState(false);
    const [institution, setInstitution] = useState(0);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const importAccount: ImportAccount | undefined = accountController === "Import" ? { importerTypeId: importerTypeId } : undefined;

        const account: Account = {
            id: emptyGuid,
            name: name,
            description: description,
            currentBalance: balance,
            accountType: accountType,
            controller: accountController,
            balanceDate: new Date(),
            accountGroupId: accountGroupId === "" ? undefined : accountGroupId,
            calculatedBalance: balance,
            lastTransaction: undefined,
            shareWithFamily: shareWithFamily,
            institutionId: institution,
            virtualAccounts: [],
        };

        createAccount.create(account, importAccount);

        navigate("/");
    }

    return (
        <Page title="Create Account" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Account", route: "/accounts/create" }]}>
            <Form className="section" onSubmit={handleSubmit}>
                <Form.Group controlId="accountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={name} onChange={(e: any) => setName(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="accountDescription" >
                    <Form.Label>Description</Form.Label>
                    <Form.Control type="text" as="textarea" required maxLength={255} value={description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                </Form.Group>
                <Form.Group>
                    <Form.Label>Institution</Form.Label>
                    <Form.Select value={institution.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setInstitution(Number(e.currentTarget.value))}>
                        <option value="">Select an institution...</option>
                        {institutions?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <Form.Group controlId="openingBalance" >
                    <Form.Label>Opening Balance</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control type="number" required value={balance.toString()} onChange={(e: any) => setBalance(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="group">
                    <Form.Label>Group</Form.Label>
                    <Form.Control as="select" value={accountGroupId} onChange={(e: any) => setAccountGroupId(e.currentTarget.value)}>
                        <option value="">Select a group...</option>
                        {accountGroups?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <Form.Group controlId="AccountType" >
                    <Form.Label>Type</Form.Label>
                    <Form.Select value={accountType.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setAccountType(e.currentTarget.value as AccountType)}>
                        {AccountTypes.map(a =>
                            <option value={a} key={a}>{a}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <Form.Group controlId="AccountController">
                    <Form.Label>Controller</Form.Label>
                    <Form.Control as="select" value={accountController.toString()} onChange={(e: any) => setAccountController(e.currentTarget.value as AccountController)}>
                        {AccountControllers.map(a =>
                            <option value={a} key={a}>{a}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <ImportSettings show={accountController === "Import"} selectedId={importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                <Form.Group controlId="ShareWithFamily">
                    <Form.Label>Visible to other family members</Form.Label>
                    <Form.Check checked={shareWithFamily} onChange={(e: any) => setShareWithFamily(e.currentTarget.checked)} />
                </Form.Group>
                <Button type="submit" variant="primary">Create</Button>
            </Form>
        </Page>
    );
}

CreateAccount.displayName = "CreateAccount";