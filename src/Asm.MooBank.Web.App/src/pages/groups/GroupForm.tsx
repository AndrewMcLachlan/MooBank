import React, { useEffect, useState } from "react";
import { Group } from "models";
import { useNavigate } from "react-router";
import { useCreateGroup, useUpdateGroup } from "services";
import { emptyGuid, NavItem, Page, Section } from "@andrewmclachlan/mooapp";
import { Button, Form } from "react-bootstrap";
import { FormGroup, FormRow } from "components";

export interface GroupFormProps {
    group?: Group
}

export const GroupForm: React.FC<GroupFormProps> = ({ group }) => {
    const navigate = useNavigate();

    const [name, setName] = useState(group?.name ?? "");
    const [description, setDescription] = useState(group?.description ?? "");
    const [showTotal, setShowTotal] = useState(group?.showTotal ?? true);

    useEffect(() => {
        setName(group?.name ?? "");
        setDescription(group?.description ?? "");
        setShowTotal(group?.showTotal ?? true);
    }, [group]);

    const createGroup = useCreateGroup();
    const updateGroup = useUpdateGroup();

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        const newGroup: Group = {
            id: group!.id,
            name: name,
            description: description,
            showTotal: showTotal,
        };

        if (group!.id === emptyGuid) {
            createGroup(newGroup);
        } else {
            updateGroup(newGroup);
        }

        navigate("/groups");
    }

    const verb = group?.id === emptyGuid ? "Create" : "Manage";
    const breadcrumb: NavItem[] = !group ? [] : group?.id === emptyGuid ? [{text: "Create Group", route: "/groups/create"}] : [{text: group?.name, route: `/groups/${group?.id}/manage`}];

    return (
        <Page title={`${verb} Group`} breadcrumbs={[{ text: "Groups", route: "/groups"}, ...breadcrumb]}>
            {group && <Section>
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
