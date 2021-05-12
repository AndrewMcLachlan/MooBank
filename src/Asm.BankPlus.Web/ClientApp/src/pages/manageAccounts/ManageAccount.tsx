import React, { useEffect, useState } from "react"
import { Form, InputGroup, Button } from "react-bootstrap"
import { PageHeader } from "../../components"
import { AccountController, AccountType, ImportAccount } from "../../models"
import { ImportSettings } from "../createAccount/ImportSettings"
import * as Models from "../../models";
import { toNameValue } from "../../extensions"
import { useAccount, useUpdateAccount } from "../../services"
import { useParams } from "react-router"
import { usePageTitle } from "../../hooks"

export const ManageAccount = (props: ManageAccountProps) => {

    const accountTypes = toNameValue(AccountType);
    const accountControllers = toNameValue(AccountController);

    const { id } = useParams<any>()

    const accountQuery = useAccount(id);

    const [account, setAccount] = useState<Models.Account>();
    usePageTitle(account?.name);

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
        <>
            <PageHeader title="Manage Account" breadcrumbs={[["Manage Accounts", "/accounts"], [account?.name, `/accounts/${account.id}`]]} />
            <Form onSubmit={handleSubmit}>
                <Form.Group controlId="AccountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={account.name} onChange={(e: any) => setName(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="AccountDescription" >
                    <Form.Label >Description</Form.Label>
                    <Form.Control type="text" as="textarea" required maxLength={255} value={account.description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="IncludeInPosition" >
                    <Form.Label htmlFor="includeInPosition">Include in Position</Form.Label>
                    <Form.Switch id="includeInPosition" checked={account.includeInPosition} onChange={(e) => setIncludeInPosition(e.currentTarget.checked)} />
                </Form.Group>
                <Form.Group controlId="AccountType" >
                    <Form.Label>Type</Form.Label>
                    <Form.Control as="select" value={account.accountType.toString()} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setAccountType(parseInt(e.currentTarget.value))}>
                        {accountTypes.map(a =>
                            <option value={a.value} key={a.value}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <Form.Group controlId="AccountController">
                    <Form.Label>Controller</Form.Label>
                    <Form.Control as="select" value={account.controller.toString()} onChange={(e: any) => setAccountController(parseInt(e.currentTarget.value))}>
                        {accountControllers.map(a =>
                            <option value={a.value} key={a.value}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <ImportSettings show={account.controller === AccountController.Import} selectedId={account.importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                <Button type="submit" variant="primary">Update</Button>
            </Form>
        </>
    );
}

export interface ManageAccountProps {
    account: Models.Account;
}