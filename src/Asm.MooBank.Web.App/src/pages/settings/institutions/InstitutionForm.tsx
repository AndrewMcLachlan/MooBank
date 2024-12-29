import { ComboBox } from "@andrewmclachlan/mooapp";
import { Institution, institutionTypeOptions } from "models";
import { useEffect, useState } from "react";
import { Button, Form } from "react-bootstrap";

export const InstitutionForm: React.FC<InstitutionFormProps> = ({ onSave, buttonText, institution: originalInstitution }) => {

    const [institution, setInstitution] = useState<Institution>(originalInstitution);

    useEffect(() => {
        setInstitution(originalInstitution);
    }, [originalInstitution]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        onSave(institution);
    }


    return (
        <Form className="section" onSubmit={handleSubmit}>
            <Form.Group controlId="name">
                <Form.Label>Name</Form.Label>
                <Form.Control type="text" required maxLength={50} value={institution.name} onChange={(e: any) => setInstitution({ ...institution, name: e.currentTarget.value })} />
                <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
            </Form.Group>
            <Form.Group controlId="type">
                <Form.Label>Institution Type</Form.Label>
                <ComboBox items={institutionTypeOptions} selectedItems={[institutionTypeOptions?.find(i => i.value === institution.institutionType)]} labelField={t => t.label} valueField={t => t.value} onChange={(v: any) => setInstitution({ ...institution, institutionType: v.value })} />
                <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
            </Form.Group>
            <Button type="submit" variant="primary">{buttonText}</Button>
        </Form>
    );
}

export interface InstitutionFormProps {
    institution: Institution;
    buttonText: "Add" | "Update";
    onSave: (institution: Institution) => void;
}
