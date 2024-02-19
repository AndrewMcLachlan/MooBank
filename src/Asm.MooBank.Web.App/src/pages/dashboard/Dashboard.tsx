import React from "react";

import { Dashboard as DashboardPage, DashboardProps } from "@andrewmclachlan/mooapp";
import { InOutWidget } from "components/Dashboard";
import { PositionWidget } from "components/Dashboard";
import { BudgetWidget } from "components/Dashboard/Budget";

const props: DashboardProps = {
    title: "Home",
}

export const Dashboard: React.FC = () => (
    <DashboardPage {...props}>
        <InOutWidget />
        <BudgetWidget />
        <PositionWidget />
    </DashboardPage>
);
