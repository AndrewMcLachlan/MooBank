import { createFileRoute } from "@tanstack/react-router";
import React from "react";

import { Dashboard as DashboardPage, DashboardProps } from "@andrewmclachlan/moo-app";
import { InOutWidget } from "./-widgets/InOut";
import { SummaryWidget } from "./-widgets/Summary";
import { BudgetWidget } from "./-widgets/Budget";
import { TopTagsWidget } from "./-widgets/TopTags";
import { BreakdownWidget } from "./-widgets/Breakdown";

export const Route = createFileRoute("/")({
    component: Dashboard,
});

const props: DashboardProps = {
    title: "Home",
}

function Dashboard() {
    return (
        <DashboardPage {...props}>
            <InOutWidget />
            <BudgetWidget />
            <SummaryWidget />
            <BreakdownWidget />
            <TopTagsWidget />
        </DashboardPage>
    );
}
