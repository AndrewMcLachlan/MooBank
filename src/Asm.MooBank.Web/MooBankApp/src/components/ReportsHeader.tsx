import React from "react";

import { Account } from "../models";
import { PageHeader } from "../layouts/PageHeader";
import { MenuItem } from "../models/MenuItem";
import { useAccount } from "./AccountProvider";

export const ReportsHeader: React.FC<ReportsHeaderProps> = ({account}) => {

    if (!account) return null;

    const { getMenuItems } = getRenderers(account);

    return (
        <PageHeader title={account.name} menuItems={getMenuItems()} breadcrumbs={[[account.name, `/accounts/${account.id}`], ["Reports", `/accounts/${account.id}/reports`]]} />
    )
}

ReportsHeader.displayName = "ReportsHeader";

const getRenderers = (account: Account) => {

    const getMenuItems = () => {

        if (!account) return [];

        const items: MenuItem[] = [
            { route: `/account/${account.id}/reports/in-out`, text: "Incoming v Outgoing" },
        ];

        return items;
    };

    return {
        getMenuItems,
    };
}

export interface ReportsHeaderProps {
    account?: Account;
}