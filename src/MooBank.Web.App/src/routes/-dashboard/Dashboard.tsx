import React from "react";

import { Dashboard as DashboardPage } from "@andrewmclachlan/moo-app";
import type { DashboardProps } from "@andrewmclachlan/moo-app";
import { InOutWidget } from "./InOut";
import { SummaryWidget } from "./Summary";
import { BudgetWidget } from "./Budget";
import { TopTagsWidget } from "./TopTags";
import { BreakdownWidget } from "./Breakdown";
import { ForecastWidget } from "./Forecast";

const props: DashboardProps = {
    title: "Home",
}

export function Dashboard() {
    return (
        <DashboardPage {...props}>
            <InOutWidget />
            <BudgetWidget />
            <SummaryWidget />
            <BreakdownWidget />
            <TopTagsWidget />
            <ForecastWidget />
        </DashboardPage>
    );
}
