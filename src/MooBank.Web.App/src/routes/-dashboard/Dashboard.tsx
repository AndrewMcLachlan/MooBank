import React from "react";

import { Dashboard as DashboardPage } from "@andrewmclachlan/moo-app";
import { IconLinkButton } from "@andrewmclachlan/moo-ds";
import { PiggyBank } from "@andrewmclachlan/moo-icons";
import { useAccounts } from "hooks/useAccounts";
import { InOutWidget } from "./InOut";
import { SummaryWidget } from "./Summary";
import { BudgetWidget } from "./Budget";
import { TopTagsWidget } from "./TopTags";
import { BreakdownWidget } from "./Breakdown";
import { ForecastWidget } from "./Forecast";

export function Dashboard() {

    const { data: accounts } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    const actions = account ? [
        <IconLinkButton key="primary-account" variant="primary" to={`/accounts/${account.id}/transactions`} icon={PiggyBank}>{account.name}</IconLinkButton>
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
