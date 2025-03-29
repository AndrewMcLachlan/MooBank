import React, { useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { SectionForm, Form, emptyGuid } from "@andrewmclachlan/mooapp";

import { VirtualAccount } from "../../models";
import { useCreateVirtualAccount } from "../../services";
import { AccountPage, useAccount } from "components";

export const CreateVirtualAccount = () => {

    const navigate = useNavigate();

    const parentAccount = useAccount();

    const createVirtualAccount = useCreateVirtualAccount();

    const handleSubmit = (data: VirtualAccount) => {

        createVirtualAccount(parentAccount.id, data);

        navigate(`/accounts/${parentAccount.id}/manage/`);
    }

    const form = useForm<VirtualAccount>();

    return (
        <AccountPage title="Create Virtual Account" breadcrumbs={[{ text: "Manage", route: `/accounts/${parentAccount?.id}/manage` }, { text: "Create Virtual Account", route: `/accounts/${parentAccount?.id}/manage/virtual/create` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="AccountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="AccountDescription" >
                    <Form.Label >Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="OpeningBalance" >
                    <Form.Label>Opening Balance</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Input type="number" required />
                    </InputGroup>
                </Form.Group>
                <Button type="submit" variant="primary">Create</Button>
            </SectionForm>
        </AccountPage>
    );
}

CreateVirtualAccount.displayName = "CreateVirtualAccount";
