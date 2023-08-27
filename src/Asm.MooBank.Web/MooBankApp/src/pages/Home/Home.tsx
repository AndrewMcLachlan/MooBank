import "./Dashboard.scss";

import React from "react";

import { AccountList } from "../../components";
import { Page } from "@andrewmclachlan/mooapp";
import { InOutWidget } from "components/Dashboard";
import { Col, Row } from "react-bootstrap";

export const Home: React.FC = () => (
    <Page title="Home" className="dashboard">
        <InOutWidget />
    </Page>
);
