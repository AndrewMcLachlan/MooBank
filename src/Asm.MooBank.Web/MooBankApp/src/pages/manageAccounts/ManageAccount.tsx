import React, { useEffect, useState } from "react";
import { Form, Button, Table } from "react-bootstrap";
import { AccountController, AccountType } from "models";
import { ImportSettings } from "../createAccount/ImportSettings";
import * as Models from "models";
import { toNameValue } from "extensions";
import { useAccountGroups, useInstitutions, useUpdateAccount, useVirtualAccounts } from "services";
import { useNavigate, useParams } from "react-router-dom";
import { IconButton, Section, useIdParams } from "@andrewmclachlan/mooapp";
import { AccountPage, useAccount } from "components";

export const ManageAccount = () => {

    const { data: accountGroups } = useAccountGroups();
    const { data: institutions } = useInstitutions();

    const navigate = useNavigate();

    const id = useIdParams();

    const existingAccount = useAccount();
    const { data: virtualAccounts } = useVirtualAccounts(existingAccount?.id ?? id);


    useEffect(() => {
        setAccount(existingAccount ?? Models.emptyAccount);
    }, [existingAccount]);

    const [account, setAccount] = useState<Models.Account>(existingAccount ?? Models.emptyAccount);

    const updateAccount = useUpdateAccount();

    useEffect(() => {
        setAccount(account ?? Models.emptyAccount);
    }, [account]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        updateAccount.update(account);
    }

    const setName = (name: string) => setAccount({ ...account, name: name });
    const setDescription = (description: string) => setAccount({ ...account, description: description });
    const setAccountGroupId = (accountGroupId: string) => setAccount({ ...account, accountGroupId: accountGroupId });
    const setAccountType = (accountType: AccountType) => setAccount({ ...account, accountType: accountType });
    const setAccountController = (accountController: AccountController) => setAccount({ ...account, controller: accountController });
    const setImporterTypeId = (importerTypeId: number) => setAccount({ ...account, importerTypeId: importerTypeId });
    const setShareWithFamily = (shareWithFamily: boolean) => setAccount({ ...account, shareWithFamily: shareWithFamily });
    const setInstitution = (institutionId: number) => setAccount({ ...account, institutionId: institutionId });

    return (
        <AccountPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/accounts/${account.id}/manage` }]} actions={[<IconButton key="nva" onClick={() => navigate(`/accounts/${id}/manage/virtual/create`)} icon="plus">New Virtual Account</IconButton>]}>
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
                                <Form.Label >Description</Form.Label>
                                <Form.Control type="text" as="textarea" maxLength={255} value={account.description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                                <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                            </Form.Group>
                            <Form.Group>
                                <Form.Label>Institution</Form.Label>
                                <Form.Select value={account.institutionId.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setInstitution(Number(e.currentTarget.value))}>
                                    {institutions?.map(a =>
                                        <option value={a.id} key={a.id}>{a.name}</option>
                                    )}
                                </Form.Select>
                            </Form.Group>
                            <Form.Group controlId="accountGroup" >
                                <Form.Label>Group</Form.Label>
                                <Form.Select value={account.accountGroupId} onChange={(e: any) => setAccountGroupId(e.currentTarget.value)}>
                                    <option value="" />
                                    {accountGroups?.map(a =>
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
                                <Form.Select value={account.controller} onChange={(e: any) => setAccountController(e.currentTarget.value as AccountController)}>
                                    {Models.AccountControllers.map(a =>
                                        <option value={a} key={a}>{a}</option>
                                    )}
                                </Form.Select>
                            </Form.Group>
                            <ImportSettings show={account.controller === "Import"} selectedId={account.importerTypeId} onChange={(e) => setImporterTypeId(e)} />
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
    account: Models.Account;
}