import React from "react";
import { Container } from "react-bootstrap";

export type PageContentComponent = React.FC<React.PropsWithChildren<{}>>;

export const PageContent:PageContentComponent = (props) => (
    <Container>
        {props.children}
    </Container>
)