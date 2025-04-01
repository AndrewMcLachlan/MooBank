import { Form, FormComboBox, SectionForm } from "@andrewmclachlan/mooapp";
import { Institution, institutionTypeOptions } from "models";
import { useEffect, useState } from "react";
import { Button } from "react-bootstrap";
import { useForm } from "react-hook-form";

export const InstitutionForm: React.FC<InstitutionFormProps> = ({ onSave, buttonText, institution }) => {

    const handleSubmit = (data: Institution) => {
        onSave(data);
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
            <Button type="submit" variant="primary">{buttonText}</Button>
        </SectionForm>
    );
}

export interface InstitutionFormProps {
    institution?: Institution;
    buttonText: "Add" | "Update";
    onSave: (institution: Institution) => void;
}
