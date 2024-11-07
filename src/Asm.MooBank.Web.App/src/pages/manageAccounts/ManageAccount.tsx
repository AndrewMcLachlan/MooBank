import { IconButton, Section, useIdParams } from "@andrewmclachlan/mooapp";
import { AccountPage, CurrencySelector, InstitutionSelector, useAccount } from "components";
import * as Models from "models";
import { AccountType, Controller } from "models";
import React, { useEffect, useState } from "react";
import { Button, Form, Table } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useGroups, useReprocessTransactions, useUpdateAccount, useVirtualAccounts } from "services";
import { ImportSettings } from "../createAccount/ImportSettings";

export const ManageAccount = () => {

    const { data: groups } = useGroups();
    const reprocessTransactions = useReprocessTransactions();

    const navigate = useNavigate();

    const id = useIdParams();

    const existingAccount = useAccount();
    const { data: virtualAccounts } = useVirtualAccounts(existingAccount?.id ?? id);


    useEffect(() => {
        setAccount(existingAccount as Models.InstitutionAccount ?? Models.emptyAccount);
    }, [existingAccount]);

    const [account, setAccount] = useState<Models.InstitutionAccount>(existingAccount as Models.InstitutionAccount ?? Models.emptyAccount);

    const updateAccount = useUpdateAccount();

    useEffect(() => {
        setAccount(account ?? Models.emptyAccount);
    }, [account]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        updateAccount(account);
    }

    const getActions = (accountController: Controller) => {

        const actions = [<IconButton key="nva" onClick={() => navigate(`/accounts/${id}/manage/virtual/create`)} icon="plus">New Virtual Account</IconButton>];

        if (accountController === "Import") {
            actions.push(<IconButton key="rpt" onClick={() => reprocessTransactions.mutate({ instrumentId: id })} icon="arrows-rotate">Reprocess Transactions</IconButton>);
        }

        return actions;
    }

    const setName = (name: string) => setAccount({ ...account, name: name });
    const setDescription = (description: string) => setAccount({ ...account, description: description });
    const setGroupId = (groupId: string) => setAccount({ ...account, groupId: groupId });
    const setAccountType = (accountType: AccountType) => setAccount({ ...account, accountType: accountType });
    const setAccountController = (accountController: Controller) => setAccount({ ...account, controller: accountController });
    const setImporterTypeId = (importerTypeId: number) => setAccount({ ...account, importerTypeId: importerTypeId });
    const setShareWithFamily = (shareWithFamily: boolean) => setAccount({ ...account, shareWithFamily: shareWithFamily });
    const setInstitution = (institutionId: number) => setAccount({ ...account, institutionId: institutionId });
    const setIncludeInBudget = (includeInBudget: boolean) => setAccount({ ...account, includeInBudget: includeInBudget });
    const setCurrency = (currency: string) => setAccount({ ...account, currency: currency });

    return (
        <AccountPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/accounts/${account.id}/manage` }]} actions={getActions(account.controller)}>
            {account &&
                <>
                    <Section>
                        <Form onSubmit={handleSubmit}>
                            <Form.Group controlId="AccountName">
                                <Form.Label>Name</Form.Label>
                                <Form.Control type="text" required maxLength={50} value={account.name} onChange={(e: any) => setName(e.currentTarget.value)} />
                                <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="AccountDescription">
                                <Form.Label>Description</Form.Label>
                                <Form.Control type="text" as="textarea" maxLength={255} value={account.description ?? ""} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                                <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group controlId="currency">
                                <Form.Label>Currency</Form.Label>
                                <CurrencySelector value={account.currency} onChange={code => setCurrency(code)} />
                            </Form.Group>
                            <Form.Group>
                                <Form.Label>Institution</Form.Label>
                                <InstitutionSelector accountType={account.accountType} value={account.institutionId} onChange={id => setInstitution(id)} />
                            </Form.Group>
                            <Form.Group controlId="group" >
                                <Form.Label>Group</Form.Label>
                                <Form.Select value={account.groupId} onChange={(e: any) => setGroupId(e.currentTarget.value)}>
                                    <option value="" />
                                    {groups?.map(a =>
                                        <option value={a.id} key={a.id}>{a.name}</option>
                                    )}
                                </Form.Select>
                            </Form.Group>
                            <Form.Group controlId="AccountType" >
                                <Form.Label>Type</Form.Label>
                                <Form.Select value={account.accountType} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setAccountType(e.currentTarget.value as AccountType)}>
                                    {Models.AccountTypes.map(a =>
                                        <option value={a} key={a}>{a}</option>
                                    )}
                                </Form.Select>
                            </Form.Group>
                            <Form.Group controlId="AccountController">
                                <Form.Label>Controller</Form.Label>
                                <Form.Select value={account.controller} onChange={(e: any) => setAccountController(e.currentTarget.value as Controller)}>
                                    {Models.Controllers.map(a =>
                                        <option value={a} key={a}>{a}</option>
                                    )}
                                </Form.Select>
                            </Form.Group>
                            <ImportSettings show={account.controller === "Import"} selectedId={account.importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                            <Form.Group controlId="IncludeInBudget">
                                <Form.Label>Include this account in the budget</Form.Label>
                                <Form.Check checked={account.includeInBudget} onChange={(e: any) => setIncludeInBudget(e.currentTarget.checked)} />
                            </Form.Group>
                            <Form.Group controlId="ShareWithFamily">
                                <Form.Label>Visible to other family members</Form.Label>
                                <Form.Check checked={account.shareWithFamily} onChange={(e: any) => setShareWithFamily(e.currentTarget.checked)} />
                            </Form.Group>
                            <Button type="submit" variant="primary">Update</Button>
                        </Form>
                    </Section>
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
                                    <tr key={a.id} className="clickable" onClick={() => navigate(`/accounts/${account.id}/manage/virtual/${a.id}`)}>
                                        <td>{a.name}</td>
                                        <td>{a.description}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                    </Section>
                </>
            }
        </AccountPage>
    );
}

export interface ManageAccountProps {
    account: Models.InstitutionAccount;
}
