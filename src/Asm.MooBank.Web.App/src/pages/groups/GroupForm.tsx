import React, { useEffect, useState } from "react";
import { Group } from "models";
import { useNavigate } from "react-router";
import { useCreateGroup, useUpdateGroup } from "services";
import { emptyGuid, Form, NavItem, Page, SectionForm } from "@andrewmclachlan/mooapp";
import { Button, } from "react-bootstrap";
import { useForm } from "react-hook-form";

export interface GroupFormProps {
    group?: Group
}

export const GroupForm: React.FC<GroupFormProps> = ({ group }) => {
    const navigate = useNavigate();

    const createGroup = useCreateGroup();
    const updateGroup = useUpdateGroup();

    const handleSubmit = (data: Group) => {

        const newGroup: Group = { 
            ...data,
            id: group?.id,
        };

        if (newGroup.id === emptyGuid) {
            createGroup(newGroup);
        } else {
            updateGroup(newGroup);
        }

        navigate("/groups");
    }

    const verb = group?.id === emptyGuid ? "Create" : "Manage";
    const breadcrumb: NavItem[] = !group ? [] : group?.id === emptyGuid ? [{ text: "Create Group", route: "/groups/create" }] : [{ text: group?.name, route: `/groups/${group?.id}/manage` }];

    console.log("GroupForm", group, verb, breadcrumb);
    const form = useForm<Group>({ defaultValues: group });

    useEffect(() => {
        form.reset(group);
    }, [group, form]);

    return (
        <Page title={`${verb} Group`} breadcrumbs={[{ text: "Groups", route: "/groups" }, ...breadcrumb]}>
            <SectionForm form={form} onSubmit={handleSubmit}>
                <Form.Group groupId="name" >
                    <Form.Label>Name</Form.Label>
                    <Form.Input required maxLength={50} />
                </Form.Group>
                <Form.Group groupId="description">
                    <Form.Label>Description</Form.Label>
                    <Form.TextArea maxLength={4000} />
                </Form.Group>
                <Form.Group groupId="showPosition">
                    <Form.Label>Show Total for Group</Form.Label>
                    <Form.Check />
                </Form.Group>
                <Button type="submit" variant="primary">Save</Button>
            </SectionForm>
        </Page>
    );
}
