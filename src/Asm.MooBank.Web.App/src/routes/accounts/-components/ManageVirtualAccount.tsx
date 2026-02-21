import React, { useEffect, useState } from "react";
import { Button, InputGroup } from "@andrewmclachlan/moo-ds";
import { useMatchRoute, useNavigate, useParams } from "@tanstack/react-router";
import { Form, Section, SectionForm } from "@andrewmclachlan/moo-ds";
import type { VirtualInstrument } from "api/types.gen";
import { useUpdateVirtualInstrument, useVirtualInstrument } from "services";
import { AccountPage } from "components";
import { RecurringTransactions } from "./RecurringTransactions";
import { useForm } from "react-hook-form";

export const ManageVirtualAccount = () => {

    const navigate = useNavigate();

    const { id, virtualId } = useParams({ strict: false });

    const { data: account } = useVirtualInstrument(id, virtualId);

    const matchRoute = useMatchRoute();
    const isDirect = matchRoute({ to: "/accounts/$id/virtual/$virtualId/manage" });

    const updateVirtualInstrument = useUpdateVirtualInstrument();

    const handleSubmit = (data: VirtualInstrument) => {

        updateVirtualInstrument(id, data);

        navigate({ to: `/accounts/${id}/manage/` });
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
            { account && account.controller === "Virtual" &&
            <RecurringTransactions account={account} />
}
        </AccountPage>
    );
}

ManageVirtualAccount.displayName = "MamageVirtualAccount";
