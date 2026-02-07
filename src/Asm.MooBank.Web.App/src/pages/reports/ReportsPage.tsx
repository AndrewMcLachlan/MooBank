import React, { PropsWithChildren } from "react";

import { LogicalAccount } from "models";
import { useLocation } from "react-router";
import { NavItem } from "@andrewmclachlan/moo-ds";
import { AccountPage, useAccount } from "components";
import { BarChart, LeftRightArrow, PieChart, Tags, Trendline } from "@andrewmclachlan/moo-icons";

export const ReportsPage: React.FC<PropsWithChildren<ReportsHeaderProps>> = ({ children, title }) => {

    const account = useAccount();

    const location = useLocation();

    if (!account) return  null;

    return (
        <AccountPage navItems={getMenuItems(account as LogicalAccount)} breadcrumbs={[{ text: "Reports", route: `/accounts/${account.id}/reports` }, { text: title, route: location.pathname }]} title={title}>
            {children}
        </AccountPage>
    )
}

ReportsPage.displayName = "ReportsHeader";

const getMenuItems = (account: LogicalAccount) => {

    if (!account) return [];

    const items: NavItem[] = [
        { route: `/accounts/${account.id}/reports/in-out`, text: "Income vs Expenses", image: <LeftRightArrow/> },
        { route: `/accounts/${account.id}/reports/all-tag-average`, text: "Top Tags", image: <BarChart /> },
        { route: `/accounts/${account.id}/reports/breakdown`, text: "Breakdown", image: <PieChart /> },
        { route: `/accounts/${account.id}/reports/tag-trend`, text: "Tag Trend", image: <Trendline /> },
        { route: `/accounts/${account.id}/reports/by-tag`, text: "All Tags", image: <Tags /> },
        { route: `/accounts/${account.id}/reports/monthly-balances`, text: "Monthly Balances", image: <Trendline /> },
    ];

    return items;
};

export interface ReportsHeaderProps {
    title: string;
}
