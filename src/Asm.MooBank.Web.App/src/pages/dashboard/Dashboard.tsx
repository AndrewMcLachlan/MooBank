import "./Dashboard.scss";

import React from "react";

import { Page } from "@andrewmclachlan/mooapp";
import { InOutWidget } from "components/Dashboard";
import { PositionWidget } from "components/Dashboard";
import { BudgetWidget } from "components/Dashboard/Budget";
import { Row } from "react-bootstrap";

export const Dashboard: React.FC = () => (
    <Page title="Home" className="dashboard">
        <Row>
            <InOutWidget />
            <BudgetWidget />
            <PositionWidget />
        </Row>
    </Page>
);
