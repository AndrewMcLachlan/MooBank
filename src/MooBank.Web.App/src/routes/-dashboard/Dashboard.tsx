import React from "react";

import { Dashboard as DashboardPage } from "@andrewmclachlan/moo-app";
import { PiggyBank } from "@andrewmclachlan/moo-icons";
import { useAccounts } from "hooks/useAccounts";
import { InOutWidget } from "./InOut";
import { SummaryWidget } from "./Summary";
import { BudgetWidget } from "./Budget";
import { TopTagsWidget } from "./TopTags";
import { BreakdownWidget } from "./Breakdown";
import { ForecastWidget } from "./Forecast";
import type { PageAction } from "@andrewmclachlan/moo-app";

export function Dashboard() {

    const { data: accounts } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    const actions: PageAction[] = account ? [
        { id: "primary-account", label: account.name, icon: PiggyBank, variant: "primary", to: `/accounts/${account.id}/transactions` },
    ] : [];

    return (
        <DashboardPage title="Home" actions={actions}>
            <InOutWidget />
            <BudgetWidget />
            <SummaryWidget />
            <BreakdownWidget />
            <TopTagsWidget />
            <ForecastWidget />
        </DashboardPage>
    );
}
