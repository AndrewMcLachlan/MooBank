import React from "react";
import { Container } from "reactstrap";

import Footer from "./Footer";
import Header from "./Header";

export default (props) => (
    <div id="body">
        <Header />
        <Container>
            {props.children}
        </Container>
        <Footer />
    </div>
);
