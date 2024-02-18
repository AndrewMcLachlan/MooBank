import React from "react";

import { Dashboard as DashboardPage } from "@andrewmclachlan/mooapp";
import { InOutWidget } from "components/Dashboard";
import { PositionWidget } from "components/Dashboard";
import { BudgetWidget } from "components/Dashboard/Budget";

export const Dashboard: React.FC = () => (
    <DashboardPage title="Home">
        <InOutWidget />
        <BudgetWidget />
        <PositionWidget />
    </DashboardPage>
);
