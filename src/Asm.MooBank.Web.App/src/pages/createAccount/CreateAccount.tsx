import { Page, emptyGuid } from "@andrewmclachlan/mooapp";
import { useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";

import { AccountController, AccountControllers, AccountType, AccountTypes, ImportAccount, InstitutionAccount } from "models";
import { useGroups, useCreateAccount, useInstitutions } from "services";
import { ImportSettings } from "./ImportSettings";
import { CurrencySelector } from "components";

export const CreateAccount: React.FC = () => {

    const navigate = useNavigate();

    const { data: groups } = useGroups();
    const { data: institutions } = useInstitutions();
    const createAccount = useCreateAccount();

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);
    const [accountType, setAccountType] = useState<AccountType>("Transaction");
    const [accountController, setAccountController] = useState<AccountController>("Manual");
    const [importerTypeId, setImporterTypeId] = useState(0);
    const [groupId, setgroupId] = useState("");
    const [includeInBudget, setIncludeInBudget] = useState(false);
    const [shareWithFamily, setShareWithFamily] = useState(false);
    const [institution, setInstitution] = useState(0);
    const [currency, setCurrency] = useState("AUD");

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const account: InstitutionAccount = {
            id: emptyGuid,
            name: name,
            description: description,
            currency: currency,
            currentBalance: balance,
            currentBalanceLocalCurrency: balance,
            accountType: accountType,
            controller: accountController,
            balanceDate: new Date(),
            groupId: groupId === "" ? undefined : groupId,
            calculatedBalance: balance,
            lastTransaction: undefined,
            shareWithFamily: shareWithFamily,
            institutionId: institution,
            includeInBudget: includeInBudget,
            importerTypeId: accountController === "Import" ? importerTypeId : undefined,
            virtualAccounts: [],
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
                <Form.Group>
                    <Form.Label>Institution</Form.Label>
                    <Form.Select value={institution.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setInstitution(Number(e.currentTarget.value))}>
                        <option value="">Select an institution...</option>
                        {institutions?.map(a =>
                            <option value={a.id} key={a.id}>{a.name}</option>
                        )}
                    </Form.Select>
                </Form.Group>
                <Form.Group controlId="currency" onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setCurrency(e.currentTarget.value)}>
                    <Form.Label>Currency</Form.Label>
                    <CurrencySelector value={currency} />
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
                    <Form.Control as="select" value={groupId} onChange={(e: any) => setgroupId(e.currentTarget.value)}>
                        <option value="">Select a group...</option>
                        {groups?.map(a =>
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
