import React from "react";
import { PropsWithChildren } from "react";
import { Row } from "@andrewmclachlan/moo-ds";

export const FormRow: React.FC<PropsWithChildren<unknown>> = ({children}) => (
    <Row className="mb-3">
        {children}
    </Row>
);