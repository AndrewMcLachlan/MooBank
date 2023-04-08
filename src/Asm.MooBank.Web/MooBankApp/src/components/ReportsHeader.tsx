import React from "react";

import { Account } from "../models";
import { PageHeader } from "../layouts/PageHeader";
import { MenuItem } from "../models/MenuItem";
import { useAccount } from "./AccountProvider";
import { useLocation } from "react-router";

export const ReportsHeader: React.FC<ReportsHeaderProps> = ({account, title}) => {

    const location = useLocation();

    if (!account) return null;

    const { getMenuItems } = getRenderers(account);

    return (
        <PageHeader title={account.name} menuItems={getMenuItems()} breadcrumbs={[[account.name, `/accounts/${account.id}`], ["Reports", `/accounts/${account.id}/reports`], [title, location.pathname]]} />
    )
}

ReportsHeader.displayName = "ReportsHeader";

const getRenderers = (account: Account) => {

    const getMenuItems = () => {

        if (!account) return [];

        const items: MenuItem[] = [
            { route: `/accounts/${account.id}/reports/in-out`, text: "Incoming vs Expenses" },
            { route: `/accounts/${account.id}/reports/breakdown`, text: "Breakdown" },
        ];

        return items;
    };

    return {
        getMenuItems,
    };
}

export interface ReportsHeaderProps {
    account?: Account;
    title: string;
}