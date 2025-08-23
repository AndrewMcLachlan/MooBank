import React from "react";

import { Dashboard as DashboardPage, DashboardProps } from "@andrewmclachlan/moo-app";
import { InOutWidget } from "./widgets";
import { SummaryWidget } from "./widgets";
import { BudgetWidget } from "./widgets";
import { TopTagsWidget } from "./widgets";
import { BreakdownWidget } from "./widgets";

const props: DashboardProps = {
    title: "Home",
}

export const Dashboard: React.FC = () => (
    <DashboardPage {...props}>
        <InOutWidget />
        <BudgetWidget />
        <SummaryWidget />
        <BreakdownWidget />
        <TopTagsWidget />
    </DashboardPage>
);
