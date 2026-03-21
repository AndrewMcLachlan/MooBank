import React from "react";
import type { PropsWithChildren } from "react";
import { Row } from "@andrewmclachlan/moo-ds";

export const FormRow: React.FC<PropsWithChildren<unknown>> = ({children}) => (
    <Row className="mb-3">
        {children}
    </Row>
);