import React from "react";
import { Container } from "react-bootstrap";

export type PageContentComponent = React.FC;

export const PageContent: React.FC = (props) => (
    <Container>
        {props.children}
    </Container>
)