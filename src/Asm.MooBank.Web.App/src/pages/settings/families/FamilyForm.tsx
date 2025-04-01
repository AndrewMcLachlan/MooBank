import { useEffect } from "react";
import { Button } from "react-bootstrap";
import { useForm } from "react-hook-form";

import { Form, SectionForm } from "@andrewmclachlan/mooapp";

import { Family } from "models";


export const FamilyForm: React.FC<FamilyFormProps> = ({ family, buttonText, onSave }) => {

    const handleSubmit = (data: Family) => {
        onSave(data);
    }

    const form = useForm<Family>({ defaultValues: family });

    useEffect(() => {
        form.reset(family);
    }, [family, form]);

    return (
        <SectionForm form={form} onSubmit={handleSubmit}>
            <Form.Group groupId="name">
                <Form.Label>Name</Form.Label>
                <Form.Input required maxLength={50} />
            </Form.Group>
            <Button type="submit" variant="primary">{buttonText}</Button>
        </SectionForm>
    );
}

export interface FamilyFormProps {
    family?: Family;
    buttonText: "Add" | "Update";
    onSave: (family: Family) => void;
}