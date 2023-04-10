import React, { useEffect, useState } from "react";
import { AccountGroup } from "../../models";
import { useNavigate } from "react-router-dom";
import { useCreateAccountGroup, useUpdateAccountGroup } from "../../services";
import { emptyGuid } from "@andrewmclachlan/mooapp";
import { Page } from "../../layouts";
import { Button, Form } from "react-bootstrap";
import { FormGroup, FormRow } from "../../components";

export interface AccountGroupFormProps {
    accountGroup?: AccountGroup
}

export const AccountGroupForm: React.FC<AccountGroupFormProps> = ({ accountGroup }) => {
    const navigate = useNavigate();

    const [name, setName] = useState(accountGroup?.name ?? "");
    const [description, setDescription] = useState(accountGroup?.description ?? "");
    const [showPosition, setShowPosition] = useState(accountGroup?.showPosition ?? true);

    useEffect(() => {
        setName(accountGroup?.name ?? "");
        setDescription(accountGroup?.description ?? "");
        setShowPosition(accountGroup?.showPosition ?? true);
    }, [accountGroup]);

    const createAccountGroup = useCreateAccountGroup();
    const updateAccountGroup = useUpdateAccountGroup();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const newAccountGroup: AccountGroup = {
            id: accountGroup!.id,
            name: name,
            description: description,
            showPosition: showPosition,
        };

        if (accountGroup!.id === emptyGuid) {
            createAccountGroup.create(newAccountGroup);
        } else {
            updateAccountGroup.update(newAccountGroup);
        }

        navigate("/account-groups");
    }

    const verb = accountGroup?.id === emptyGuid ? "Create" : "Manage";
    const breadcrumb: [string, string] = !accountGroup ? ["", ""] : accountGroup?.id === emptyGuid ? ["Create Account Group", "/account-groups/create"] : [accountGroup?.name, `/account-groups/${accountGroup?.id}/manage`];

    return (
        <Page title={`${verb} Account Group`}>
            <Page.Header goBack title={`${verb} Account Group`} breadcrumbs={[["Manage Account Groups", "/account-groups"], breadcrumb]} />
            {accountGroup && <Page.Content>
                <Form onSubmit={handleSubmit}>
                    <FormRow>
                        <FormGroup controlId="name" >
                            <Form.Label>Name</Form.Label>
                            <Form.Control type="text" required maxLength={50} value={name} onChange={(e: any) => setName(e.currentTarget.value)} />
                            <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="description">
                            <Form.Label>Description</Form.Label>
                            <Form.Control type="text" as="textarea" maxLength={4000} value={description} onChange={(e: any) => setDescription(e.currentTarget.value)} />
                            <Form.Control.Feedback type="invalid">Please enter a description</Form.Control.Feedback>
                        </FormGroup>
                    </FormRow>
                    <FormRow>
                        <FormGroup controlId="showPosition">
                            <Form.Label>Show Position for Group</Form.Label>
                            <Form.Switch checked={showPosition} onChange={(e) => setShowPosition(e.currentTarget.checked)} />
                        </FormGroup>
                    </FormRow>
                    <Button type="submit" variant="primary">Save</Button>
                </Form>
            </Page.Content>}
        </Page>
    );
}