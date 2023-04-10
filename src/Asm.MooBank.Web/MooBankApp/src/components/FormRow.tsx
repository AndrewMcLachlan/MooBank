import React from "react";
import { PropsWithChildren } from "react";
import { Row } from "react-bootstrap";

export const FormRow: React.FC<PropsWithChildren<unknown>> = ({children}) => (
    <Row className="mb-3">
        {children}
    </Row>
);