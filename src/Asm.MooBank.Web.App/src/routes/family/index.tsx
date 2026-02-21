import { createFileRoute } from "@tanstack/react-router";
import { Page } from "@andrewmclachlan/moo-app";
import { DeleteIcon, Form, Section, SectionTable } from "@andrewmclachlan/moo-ds";
import type { Family } from "api/types.gen";
import React, { useEffect } from "react";
import { Button } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useMyFamily, useRemoveFamilyMember, useUpdateMyFamily, useUser } from "services";

export const Route = createFileRoute("/family/")({
    component: MyFamily,
});

function MyFamily() {

    const { data: me } = useUser();
    const { data: family } = useMyFamily();
    const updateFamily = useUpdateMyFamily();
    const removeMember = useRemoveFamilyMember();

    const form = useForm<Family>({ defaultValues: family });
    const { reset, handleSubmit } = form;

    useEffect(() => {
        reset(family);
    }, [family, reset]);

    const onSubmit = async (data: Family) => {
        updateFamily(data);
    };

    const handleRemoveMember = (userId: string) => {
        if (window.confirm("Are you sure you want to remove this family member? They will be moved to their own family.")) {
            removeMember(userId);
        }
    };

    const formatMemberName = (firstName?: string, lastName?: string, email?: string) => {
        if (firstName || lastName) {
            return `${firstName ?? ""} ${lastName ?? ""}`.trim();
        }
        return email ?? "Unknown";
    };

    return (
        <Page title="My Family" breadcrumbs={[{ text: "My Family", route: "/family" }]}>
            <Form form={form} onSubmit={handleSubmit(onSubmit)}>
                <Section header="Family Details">
                    <Form.Group groupId="name">
                        <Form.Label>Family Name</Form.Label>
                        <Form.Input type="text" maxLength={50} {...form.register("name", { required: true })} />
                    </Form.Group>
                    <Button type="submit" variant="primary">Save</Button>
                </Section>
            </Form>
            <SectionTable header="Family Members">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Email</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {family?.members?.map((member) => (
                        <tr key={member.id}>
                            <td>{formatMemberName(member.firstName, member.lastName, member.emailAddress)}</td>
                            <td>{member.emailAddress}</td>
                            <td className="row-action">
                                {member.id !== me?.id && <DeleteIcon onClick={() => handleRemoveMember(member.id)} />}
                            </td>
                        </tr>
                    ))}
                    {(!family?.members || family.members.length === 0) && (
                        <tr>
                            <td colSpan={3} className="text-center text-muted">No family members</td>
                        </tr>
                    )}
                </tbody>
            </SectionTable>
        </Page>
    );
}
