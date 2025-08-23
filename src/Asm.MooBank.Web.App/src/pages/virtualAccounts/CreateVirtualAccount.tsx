import { Button } from "react-bootstrap";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { SectionForm, Form } from "@andrewmclachlan/moo-ds";

import { CreateVirtualInstrument, VirtualAccount } from "../../models";
import { useCreateVirtualAccount } from "../../services";
import { AccountPage, CurrencyInput, useAccount } from "components";

export const CreateVirtualAccount = () => {

    const navigate = useNavigate();

    const parentAccount = useAccount();

    const createVirtualAccount = useCreateVirtualAccount();

    const handleSubmit = async (data: CreateVirtualInstrument) => {

        await createVirtualAccount.mutateAsync(parentAccount.id, data);

        navigate(`/accounts/${parentAccount.id}/manage/`);
    }

    const form = useForm<CreateVirtualInstrument>();

    return (
        <AccountPage title="Create Virtual Account" breadcrumbs={[{ text: "Manage", route: `/accounts/${parentAccount?.id}/manage` }, { text: "Create Virtual Account", route: `/accounts/${parentAccount?.id}/manage/virtual/create` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description" >
                    <Form.Label >Description</Form.Label>
                    <Form.TextArea maxLength={255} />
                </Form.Group>
                <Form.Group groupId="openingBalance" >
                    <Form.Label>Opening Balance</Form.Label>
                    <CurrencyInput />
                </Form.Group>
                <Button type="submit" variant="primary">Save</Button>
            </SectionForm>
        </AccountPage>
    );
}

CreateVirtualAccount.displayName = "CreateVirtualAccount";
