import React, { PropsWithChildren } from "react";

import { InstitutionAccount } from "models";
import { useLocation } from "react-router";
import { NavItem } from "@andrewmclachlan/mooapp";
import { AccountPage, useAccount } from "components";
import { BarChart, LeftRightArrow, PieChart, Tags, Trendline } from "@andrewmclachlan/mooicons";

export const ReportsPage: React.FC<PropsWithChildren<ReportsHeaderProps>> = ({ children, title }) => {

    const account = useAccount();

    const location = useLocation();

    if (!account) return  null;

    return (
        <AccountPage navItems={getMenuItems(account as InstitutionAccount)} breadcrumbs={[{ text: "Reports", route: `/accounts/${account.id}/reports` }, { text: title, route: location.pathname }]} title={title}>
            {children}
        </AccountPage>
    )
}

ReportsPage.displayName = "ReportsHeader";

const getMenuItems = (account: InstitutionAccount) => {

    if (!account) return [];

    const items: NavItem[] = [
        { route: `/accounts/${account.id}/reports/in-out`, text: "Income vs Expenses", image: <LeftRightArrow/> },
        { route: `/accounts/${account.id}/reports/all-tag-average`, text: "Top Tags", image: <BarChart /> },
        { route: `/accounts/${account.id}/reports/breakdown`, text: "Breakdown", image: <PieChart /> },
        { route: `/accounts/${account.id}/reports/tag-trend`, text: "Tag Trend", image: <Trendline /> },
        { route: `/accounts/${account.id}/reports/by-tag`, text: "All Tags", image: <Tags /> },
    ];

    return items;
};

export interface ReportsHeaderProps {
    title: string;
}
