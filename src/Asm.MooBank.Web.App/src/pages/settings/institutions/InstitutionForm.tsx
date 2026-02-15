import { Form, FormComboBox, SectionForm } from "@andrewmclachlan/moo-ds";
import { Institution, institutionTypeOptions } from "models";
import { useEffect, useState } from "react";
import { Button } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";
import { useCreateInstitution, useUpdateInstitution } from "services";

export const InstitutionForm: React.FC<InstitutionFormProps> = ({ institution = null }) => {

    const navigate = useNavigate();

    const createInstitution = useCreateInstitution();
    const updateInstitution = useUpdateInstitution();

    const isPending = createInstitution.isPending || updateInstitution.isPending;

    const handleSubmit = async (data: Institution) => {

        if (institution === null) {
            await createInstitution.mutateAsync(data);
        }
        else {
            await updateInstitution.mutateAsync(data);
        }

        navigate("/settings/institutions");
    }

    const form = useForm<Institution>({ defaultValues: institution });

    useEffect(() => {
        form.reset(institution);
    }, [institution, form]);

    return (
        <SectionForm form={form} onSubmit={handleSubmit}>
            <Form.Group groupId="name">
                <Form.Label>Name</Form.Label>
                <Form.Input required maxLength={50} />
            </Form.Group>
            <Form.Group groupId="institutionType">
                <Form.Label>Institution Type</Form.Label>
                <FormComboBox items={institutionTypeOptions} labelField={t => t.label} valueField={t => t.value} />
            </Form.Group>
            <Button type="submit" variant="primary" disabled={isPending}>Save</Button>
        </SectionForm>
    );
}

export interface InstitutionFormProps {
    institution?: Institution;
}
