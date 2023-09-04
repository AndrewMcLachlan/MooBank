import "./Dashboard.scss";

import React from "react";

import { Page } from "@andrewmclachlan/mooapp";
import { InOutWidget } from "components/Dashboard";

export const Dashboard: React.FC = () => (
    <Page title="Home" className="dashboard">
        <InOutWidget />
    </Page>
);
