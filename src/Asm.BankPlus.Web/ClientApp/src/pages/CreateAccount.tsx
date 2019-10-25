import React, { useState } from "react";
import { Form, Row, Button, InputGroup } from "react-bootstrap";
import { AccountType, AccountController } from "../models";
import { toNameValue } from "../extensions";
import { useDispatch } from "react-redux";
import { actionCreators } from "store/Accounts";
import { bindActionCreators } from "redux";

export const CreateAccount: React.FC = () => {

    const dispatch = useDispatch();
    bindActionCreators(actionCreators, dispatch);

    const accountTypes = toNameValue(AccountType);

    const accountControllers = toNameValue(AccountController);

const [name, setName] = useState("");
const [description, setDescription] = useState("");
const [balance, setBalance] = useState(0);
const [accountType, setAccountType] = useState(AccountType.Transaction);
const [accountController, setAccountController] = useState(AccountController.Manual);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        dispatch(actionCreators.createAccount({
            id: null,
            name: name,
            description: description,
            availableBalance: balance,
            currentBalance: balance,
            accountType: accountType,
            controller: accountController,
            balanceUpdated: new Date()
        }));
    }

    return (
        <Form onSubmit={handleSubmit}>
            <Form.Group as={Row} controlId="AccountName" >
                <Form.Label column sm="2">Name</Form.Label>
                <Form.Control type="text" required maxLength={50} value={name} onChange={(e:any) => setName(e.currentTarget.value)} />
                <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
            </Form.Group>
            <Form.Group as={Row} controlId="AccountDescription" >
                <Form.Label column sm="2">Description</Form.Label>
                <Form.Control type="text" as="textarea" required maxLength={255} value={description} onChange={(e:any) => setDescription(e.currentTarget.value)} />
                <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
            </Form.Group>
            <Form.Group as={Row} controlId="OpeningBalance" >
                <Form.Label column sm="2">Opening Balance</Form.Label>
                <InputGroup>
                    <InputGroup.Prepend>$</InputGroup.Prepend>
                    <Form.Control type="number" required defaultValue="0" value={balance.toString()} onChange={(e:any) => setBalance(e.currentTarget.value)} />
                </InputGroup>
            </Form.Group>
            <Form.Group as={Row} controlId="AccountType" >
                <Form.Label column sm="2">Type</Form.Label>
                <Form.Control as="select" onChange={(e:any) => setAccountType(e.currentTarget.value)}>
                    {accountTypes.map(a =>
                        <option selected={a.value === accountType} value={a.value}>{a.name}</option>
                    )}
                </Form.Control>
            </Form.Group>
            <Form.Group as={Row} controlId="AccountController">
                <Form.Label column sm="2">Controller</Form.Label>
                <Form.Control as="select" onChange={(e:any) => setAccountController(e.currentTarget.value)}>
                    {accountControllers.map(a =>
                        <option selected={a.value === accountController} value={a.value}>{a.name}</option>
                    )}
                </Form.Control>
            </Form.Group>
            <Button type="submit" variant="primary">Create</Button>
        </Form>
    );
}

CreateAccount.displayName = "CreateAccount";