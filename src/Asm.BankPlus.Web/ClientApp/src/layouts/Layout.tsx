import React from "react";
import { Container } from "react-bootstrap";

import { Footer } from "./Footer";
import { Header } from "./Header";

export const Layout:React.FC = (props: React.PropsWithChildren<any>) => (
    <div id="body">
        <Header />
        <Container>
            {props.children}
        </Container>
        <Footer />
    </div>
);
