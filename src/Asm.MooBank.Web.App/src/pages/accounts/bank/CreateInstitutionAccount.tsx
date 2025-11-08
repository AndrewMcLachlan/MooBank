import { Button } from "react-bootstrap";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { SectionForm, Form } from "@andrewmclachlan/moo-ds";

import { CreateInstitutionAccount as Create, InstitutionAccount, LogicalAccount } from "models";
import { useCreateInstitutionAccount } from "services";
import { AccountPage, InstitutionSelector, useAccount } from "components";
import { ImportSettings } from "../ImportSettings";

export const CreateInstitutionAccount = () => {

    const navigate = useNavigate();

    const parentAccount = useAccount() as LogicalAccount;

    const createInstitutionAccount = useCreateInstitutionAccount();

    const handleSubmit = async (data: Create) => {

        await createInstitutionAccount.mutateAsync(parentAccount.id, data);

        navigate(`/accounts/${parentAccount.id}/manage/`);
    }

    const form = useForm<Create>();

    return (
        <AccountPage title="Create Bank Account" breadcrumbs={[{ text: "Manage", route: `/accounts/${parentAccount?.id}/manage` }, { text: "Create Bank Account", route: `/accounts/${parentAccount?.id}/manage/bank/create` }]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input type="text" required maxLength={255} />
                </Form.Group>
                <Form.Group groupId="institutionId" >
                    <Form.Label>Institution</Form.Label>
                    <InstitutionSelector accountType={parentAccount?.accountType} />
                </Form.Group>
                <Form.Group groupId="importerTypeId" >
                    <Form.Label>Importer Type</Form.Label>
                    <ImportSettings />
                </Form.Group>
                <Form.Group groupId="openedDate" >
                    <Form.Label>Opened Date</Form.Label>
                    <Form.Input type="date" required defaultValue={new Date().toISOString().split("T")[0]} />
                </Form.Group>
                <Button type="submit" variant="primary">Save</Button>
            </SectionForm>
        </AccountPage>
    );
}

CreateInstitutionAccount.displayName = "CreateInstitutionAccount";
