import React, { useEffect } from "react";
import type { Group } from "api/types.gen";
import { useNavigate } from "react-router";
import { useCreateGroup, useUpdateGroup } from "services";
import { Page } from "@andrewmclachlan/moo-app";
import { emptyGuid, Form, NavItem, SectionForm } from "@andrewmclachlan/moo-ds";
import { Button, } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";

export interface GroupFormProps {
    group?: Group
}

export const GroupForm: React.FC<GroupFormProps> = ({ group }) => {
    const navigate = useNavigate();

    const createGroup = useCreateGroup();
    const updateGroup = useUpdateGroup();

    const isPending = createGroup.isPending || updateGroup.isPending;

    const handleSubmit = (data: Group) => {

        const newGroup: Group = { 
            ...data,
            id: group?.id,
        };

        if (newGroup.id === emptyGuid) {
            createGroup.mutateAsync(newGroup);
        } else {
            updateGroup.mutateAsync(newGroup);
        }

        navigate("/groups");
    }

    const verb = group?.id === emptyGuid ? "Create" : "Manage";
    const breadcrumb: NavItem[] = !group ? [] : group?.id === emptyGuid ? [{ text: "Create Group", route: "/groups/create" }] : [{ text: group?.name, route: `/groups/${group?.id}/manage` }];

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
                <Form.Group groupId="showTotal" className="form-check">
                    <Form.Check />
                    <Form.Label className="form-check-label">Show Total for Group</Form.Label>
                </Form.Group>
                <Button type="submit" variant="primary" disabled={isPending}>Save</Button>
            </SectionForm>
        </Page>
    );
}
