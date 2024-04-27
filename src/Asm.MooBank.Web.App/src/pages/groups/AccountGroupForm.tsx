import React, { useEffect, useState } from "react";
import { Group } from "models";
import { useNavigate } from "react-router-dom";
import { useCreateAccountGroup, useUpdateAccountGroup } from "services";
import { emptyGuid, NavItem, Page, Section } from "@andrewmclachlan/mooapp";
import { Button, Form } from "react-bootstrap";
import { FormGroup, FormRow } from "components";

export interface AccountGroupFormProps {
    accountGroup?: Group
}

export const AccountGroupForm: React.FC<AccountGroupFormProps> = ({ accountGroup }) => {
    const navigate = useNavigate();

    const [name, setName] = useState(accountGroup?.name ?? "");
    const [description, setDescription] = useState(accountGroup?.description ?? "");
    const [showTotal, setShowTotal] = useState(accountGroup?.showPosition ?? true);

    useEffect(() => {
        setName(accountGroup?.name ?? "");
        setDescription(accountGroup?.description ?? "");
        setShowTotal(accountGroup?.showPosition ?? true);
    }, [accountGroup]);

    const createAccountGroup = useCreateAccountGroup();
    const updateAccountGroup = useUpdateAccountGroup();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const newAccountGroup: Group = {
            id: accountGroup!.id,
            name: name,
            description: description,
            showPosition: showTotal,
        };

        if (accountGroup!.id === emptyGuid) {
            createAccountGroup(newAccountGroup);
        } else {
            updateAccountGroup(newAccountGroup);
        }

        navigate("/groups");
    }

    const verb = accountGroup?.id === emptyGuid ? "Create" : "Manage";
    const breadcrumb: NavItem[] = !accountGroup ? [] : accountGroup?.id === emptyGuid ? [{text: "Create Account Group", route: "/groups/create"}] : [{text: accountGroup?.name, route: `/groups/${accountGroup?.id}/manage`}];

    return (
        <Page title={`${verb} Group`} breadcrumbs={[{ text: "Groups", route: "/groups"}, ...breadcrumb]}>
            {accountGroup && <Section>
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
                            <Form.Label>Show Total for Group</Form.Label>
                            <Form.Switch checked={showTotal} onChange={(e) => setShowTotal(e.currentTarget.checked)} />
                        </FormGroup>
                    </FormRow>
                    <Button type="submit" variant="primary">Save</Button>
                </Form>
            </Section>}
        </Page>
    );
}