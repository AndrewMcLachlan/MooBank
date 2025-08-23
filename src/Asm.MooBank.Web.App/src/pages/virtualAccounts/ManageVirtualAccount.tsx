import React, { useEffect, useState } from "react";
import { Button, InputGroup } from "react-bootstrap";
import { useMatch, useNavigate, useParams } from "react-router";
import { Form, Section, SectionForm } from "@andrewmclachlan/moo-ds";
import { VirtualAccount } from "../../models";
import { useUpdateVirtualAccount, useVirtualAccount } from "../../services";
import { AccountPage } from "components";
import { RecurringTransactions } from "./RecurringTransactions";
import { useForm } from "react-hook-form";

export const ManageVirtualAccount = () => {

    const navigate = useNavigate();

    const { id, virtualId } = useParams<{ id: string, virtualId: string }>();

    const { data: account } = useVirtualAccount(id, virtualId);

    const isDirect = useMatch("/accounts/:id/virtual/:virtualId/manage");

    const updateVirtualAccount = useUpdateVirtualAccount();

    const handleSubmit = (data: VirtualAccount) => {

        updateVirtualAccount(id, data);

        navigate(`/accounts/${id}/manage/`);
    }

    const form = useForm({ defaultValues: account });

    useEffect(() => {
        form.reset(account);
    }, [account, form]);

    const breadcrumbs = isDirect ?
        [{ text: "Manage", route: `/accounts/${id}/virtual/${virtualId}/manage` }] :
        [{ text: "Manage", route: `/accounts/${id}/manage` }, { text: account?.name, route: `/accounts/${id}/manage/virtual/${virtualId}` }]

    return (
        <AccountPage title="Manage Virtual Account" breadcrumbs={breadcrumbs}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description" >
                    <Form.Label >Description</Form.Label>
                    <Form.TextArea required maxLength={255} />
                </Form.Group>
                <Button type="submit" variant="primary">Save</Button>
            </SectionForm>
            { account &&
            <RecurringTransactions account={account} />
}
        </AccountPage>
    );
}

ManageVirtualAccount.displayName = "MamageVirtualAccount";
