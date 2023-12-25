import React, { PropsWithChildren } from "react";

import { InstitutionAccount } from "models";
import { useLocation } from "react-router";
import { NavItem } from "@andrewmclachlan/mooapp";
import { AccountPage, useAccount } from "components";

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
        { route: `/accounts/${account.id}/reports/in-out`, text: "Income vs Expenses" },
        { route: `/accounts/${account.id}/reports/all-tag-average`, text: "All Tag Average" },
        { route: `/accounts/${account.id}/reports/breakdown`, text: "Breakdown" },
        { route: `/accounts/${account.id}/reports/by-tag`, text: "By Tag" },
        { route: `/accounts/${account.id}/reports/tag-trend`, text: "Tag Trend" },
    ];

    return items;
};

export interface ReportsHeaderProps {
    title: string;
}