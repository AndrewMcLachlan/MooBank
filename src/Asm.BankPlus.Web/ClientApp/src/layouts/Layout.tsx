import React from "react";
import { Container } from "react-bootstrap";

import { Alert } from "../components";
import { Footer } from "./Footer";
import { Header } from "./Header";

export const Layout:React.FC = (props: React.PropsWithChildren<any>) => (
    <div id="body">
        <Alert />
        <Header />
        <Container as="main">
            {props.children}
        </Container>
        <Footer />
    </div>
);
