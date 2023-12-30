import React, { useState } from "react";
import { Form, Button } from "react-bootstrap";
import { emptyFamily, Family } from "models";
import { useNavigate } from "react-router-dom";

import { useCreateFamily } from "services";
import { Page } from "@andrewmclachlan/mooapp";

export const CreateFamily: React.FC = () => {

    const navigate = useNavigate();

    const createFamily = useCreateFamily();

    const [family, setFamily] = useState<Family>(emptyFamily);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.stopPropagation();
        e.preventDefault();

        createFamily.create(family);

        navigate("/settings/family");
    }

    return (
        <Page title="Create Stock Holding" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Stock Holding", route: "/stock/create" }]}>
            <Form className="section" onSubmit={handleSubmit}>
                <Form.Group controlId="accountName" >
                    <Form.Label>Name</Form.Label>
                    <Form.Control type="text" required maxLength={50} value={family.name} onChange={(e: any) => setFamily({ ...family, name: e.currentTarget.value })} />
                    <Form.Control.Feedback type="invalid">Please enter a name</Form.Control.Feedback>
                </Form.Group>
                <Button type="submit" variant="primary">Add</Button>
            </Form>
        </Page>
    );
}

CreateFamily.displayName = "CreateFamily";
