import React, { useEffect, useState } from "react";
import { Form, Button, Row } from "react-bootstrap";
import { AccountController, AccountType } from "../../models";
import { ImportSettings } from "../createAccount/ImportSettings";
import * as Models from "../../models";
import { toNameValue } from "../../extensions";
import { useAccount, useUpdateAccount } from "../../services";
import { useParams } from "react-router-dom";
import { Page } from "../../layouts";

export const ManageAccount = () => {

    const accountTypes = toNameValue(AccountType);
    const accountControllers = toNameValue(AccountController);

    const { id } = useParams<any>()

    const accountQuery = useAccount(id);

    const [account, setAccount] = useState<Models.Account>();

    const updateAccount = useUpdateAccount();

    useEffect(() => {
        setAccount(accountQuery.data);
    }, [accountQuery.data]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        updateAccount.update(account);
    }

    const setName = (name: string) => setAccount({ ...account, name: name });
    const setDescription = (description: string) => setAccount({ ...account, description: description });
    const setIncludeInPosition = (includeInPosition: boolean) => setAccount({ ...account, includeInPosition: includeInPosition });
    const setAccountType = (accountType: AccountType) => setAccount({ ...account, accountType: accountType });
    const setAccountController = (accountController: AccountController) => setAccount({ ...account, controller: accountController });
    const setImporterTypeId = (importerTypeId: number) => setAccount({ ...account, importerTypeId: importerTypeId });

    if (!account) return <></>;

    return (
        <Page title={account?.name}>
            <Page.Header title="Manage Account" breadcrumbs={[["Manage Accounts", "/accounts"], [account?.name, `/accounts/${account.id}/manage`]]} menuItems={[{ text: "Create Virtual Account", route: `/accounts/${id}/virtual/create` }]} />
            <Page.Content>
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
                    <Form.Group controlId="IncludeInPosition" >
                        <Form.Label htmlFor="includeInPosition">Include in Position</Form.Label>
                        <Form.Switch id="includeInPosition" checked={account.includeInPosition} onChange={(e) => setIncludeInPosition(e.currentTarget.checked)} />
                    </Form.Group>
                    <Form.Group controlId="AccountType" >
                        <Form.Label>Type</Form.Label>
                        <Form.Select value={account.accountType.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setAccountType(parseInt(e.currentTarget.value))}>
                            {accountTypes.map(a =>
                                <option value={a.value} key={a.value}>{a.name}</option>
                            )}
                        </Form.Select>
                    </Form.Group>
                    <Form.Group controlId="AccountController">
                        <Form.Label>Controller</Form.Label>
                        <Form.Select value={account.controller.toString()} onChange={(e: any) => setAccountController(parseInt(e.currentTarget.value))}>
                            {accountControllers.map(a =>
                                <option value={a.value} key={a.value}>{a.name}</option>
                            )}
                        </Form.Select>
                    </Form.Group>
                    <ImportSettings show={account.controller === AccountController.Import} selectedId={account.importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                    <Button type="submit" variant="primary">Update</Button>
                </Form>
            </Page.Content>
        </Page>
    );
}

export interface ManageAccountProps {
    account: Models.Account;
}