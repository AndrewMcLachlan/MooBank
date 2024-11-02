import { Page } from "@andrewmclachlan/mooapp";
import { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";

import { CurrencySelector, InstitutionSelector } from "components";
import { AccountType, AccountTypes, Controller, Controllers, CreateInstitutionAccount } from "models";
import { useCreateAccount, useGroups } from "services";
import { ImportSettings } from "./ImportSettings";

export const CreateAccount: React.FC = () => {

    const navigate = useNavigate();

    const { data: groups } = useGroups();
    const createAccount = useCreateAccount();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);
    const [accountType, setAccountType] = useState<AccountType>("Transaction");
    const [accountController, setAccountController] = useState<Controller>("Manual");
    const [importerTypeId, setImporterTypeId] = useState(0);
    const [groupId, setgroupId] = useState("");
    const [includeInBudget, setIncludeInBudget] = useState(false);
    const [shareWithFamily, setShareWithFamily] = useState(false);
    const [institutionId, setInstitutionId] = useState<number>(undefined);
    const [currency, setCurrency] = useState("AUD");

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const account: CreateInstitutionAccount = {
            name: name,
            description: description,
            currency: currency,
            balance: balance,
            accountType: accountType,
            controller: accountController,
            groupId: groupId === "" ? undefined : groupId,
            shareWithFamily: shareWithFamily,
            institutionId: institutionId,
            includeInBudget: includeInBudget,
            importerTypeId: accountController === "Import" ? importerTypeId : undefined,
        };

        createAccount(account);

        navigate("/accounts");
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
                <Form.Group controlId="AccountType" >
                    <Form.Label>Type</Form.Label>
                    <Form.Select value={accountType.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setAccountType(e.currentTarget.value as AccountType)}>
                        {AccountTypes.map(a =>
                            <option value={a} key={a}>{a}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <Form.Group>
                    <Form.Label>Institution</Form.Label>
                    <InstitutionSelector accountType={accountType} value={institutionId} onChange={(id) => setInstitutionId(id)} required />
                </Form.Group>
                <Form.Group controlId="currency">
                    <Form.Label>Currency</Form.Label>
                    <CurrencySelector value={currency} onChange={(code) => setCurrency(code)} />
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
                    <Form.Select value={groupId} onChange={(e: any) => setgroupId(e.currentTarget.value)}>
                        <option value="">Select a group...</option>
                        {groups?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <Form.Group controlId="AccountController">
                    <Form.Label>Controller</Form.Label>
                    <Form.Select value={accountController.toString()} onChange={(e: any) => setAccountController(e.currentTarget.value as Controller)}>
                        {Controllers.map(a =>
                            <option value={a} key={a}>{a}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <ImportSettings show={accountController === "Import"} selectedId={importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                <Form.Group controlId="IncludeInBudget">
                    <Form.Label>Include this account in the budget</Form.Label>
                    <Form.Check checked={includeInBudget} onChange={(e: any) => setIncludeInBudget(e.currentTarget.checked)} />
                </Form.Group>
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
