import React from "react";
import { Form, Row } from "react-bootstrap";
import { AccountType, AccountController } from "../models";
import { toNameValue } from "../extensions";

export const CreateAccount: React.FC = () => {

    const accountTypes= toNameValue(AccountType);

    const accountControllers = toNameValue(AccountController);

    const handleSubmit = (e:React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
    }

    return (
        <Form onSubmit={handleSubmit}>
            <fieldset>
                <legend>Account Details</legend>
                <Form.Group as={Row} controlId="AccountName" >
                    <Form.Label column sm="2">Name</Form.Label>
                    <Form.Control type="text" />
                </Form.Group>
                <Form.Group as={Row} controlId="AccountDescription" >
                    <Form.Label column sm="2">Description</Form.Label>
                    <Form.Control type="text" multiple />
                </Form.Group>
                <Form.Group as={Row} controlId="OpeningBalance" >
                    <Form.Label column sm="2">Opening Balance</Form.Label>
                    <Form.Control type="number" />
                </Form.Group>
                <Form.Group as={Row} controlId="AccountType" >
                    <Form.Label column sm="2">Type</Form.Label>
                    <Form.Control as="select">
                        {accountTypes.map(a =>
                                <option value={a.value}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
                <Form.Group as={Row} controlId="AccountController" >
                    <Form.Label column sm="2">Controller</Form.Label>
                    <Form.Control as="select">
                        {accountControllers.map(a =>
                                <option value={a.value}>{a.name}</option>
                        )}
                    </Form.Control>
                </Form.Group>
            </fieldset>
        </Form>
    );
}

CreateAccount.displayName = "CreateAccount";