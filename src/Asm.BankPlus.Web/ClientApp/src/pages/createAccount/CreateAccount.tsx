import React, { useState } from "react";
import { Form, Button, InputGroup, FormControlProps } from "react-bootstrap";
import { AccountType, AccountController, ImportAccount } from "../../models";
import { toNameValue } from "../../extensions";
import { useDispatch } from "react-redux";
import { actionCreators } from "store/Accounts";
import { bindActionCreators } from "redux";
import { Redirect } from "react-router";

import { ImportSettings } from "./ImportSettings";
import { BsPrefixProps, ReplaceProps } from "react-bootstrap/helpers";
import { usePageTitle } from "hooks";

export const CreateAccount: React.FC = () => {

    usePageTitle("Create Account");

    const dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    const accountTypes = toNameValue(AccountType);

    const accountControllers = toNameValue(AccountController);

    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [balance, setBalance] = useState(0);
    const [accountType, setAccountType] = useState(AccountType.Transaction);
    const [accountController, setAccountController] = useState(AccountController.Manual);
    const [importerTypeId, setImporterTypeId] = useState(0);
    const [submitted, setSubmitted] = useState(false);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const importAccount: ImportAccount = accountController === AccountController.Import ? { importerTypeId: importerTypeId } : null;

        dispatch(actionCreators.createAccount({
            id: null,
            name: name,
            description: description,
            availableBalance: balance,
            currentBalance: balance,
            accountType: accountType,
            controller: accountController,
            balanceUpdated: new Date(),
        }, importAccount));

        setSubmitted(true);
    }

    if (submitted) {
        return <Redirect to="/" />;
    }

    return (
        <>
            <h1>Create Account</h1>
            <Form onSubmit={handleSubmit}>
                <Form.Group controlId="AccountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={name} onChange={(e: any) => setName(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="AccountDescription" >
                    <Form.Label >Description</Form.Label>
                    <Form.Control type="text" as="textarea" required maxLength={255} value={description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                    <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                </Form.Group>
                <Form.Group controlId="OpeningBalance" >
                    <Form.Label>Opening Balance</Form.Label>
                    <InputGroup>
                        <InputGroup.Prepend>$</InputGroup.Prepend>
                        <Form.Control type="number" required value={balance.toString()} onChange={(e: any) => setBalance(e.currentTarget.value)} />
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="AccountType" >
                    <Form.Label>Type</Form.Label>
                    <Form.Control as="select" value={accountType.toString()} onChange={(e: React.FormEvent<ReplaceProps<React.ElementType<any>, BsPrefixProps<React.ElementType<any>> & FormControlProps>>) => setAccountType(parseInt(e.currentTarget.value))}>
                        {accountTypes.map(a =>
                            <option value={a.value} key={a.value}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <Form.Group controlId="AccountController">
                    <Form.Label>Controller</Form.Label>
                    <Form.Control as="select" value={accountController.toString()} onChange={(e: any) => setAccountController(parseInt(e.currentTarget.value))}>
                        {accountControllers.map(a =>
                            <option value={a.value} key={a.value}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <ImportSettings show={accountController === AccountController.Import} selectedId={importerTypeId} onChange={(e) => setImporterTypeId(e)} />
                <Button type="submit" variant="primary">Create</Button>
            </Form>
        </>
    );
}

CreateAccount.displayName = "CreateAccount";