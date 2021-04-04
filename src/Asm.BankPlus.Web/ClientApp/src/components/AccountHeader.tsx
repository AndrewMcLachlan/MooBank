import "./AccountSummary.scss";

import React from "react";

import { Account, AccountController } from "../models";
import { PageHeader } from "./PageHeader";
import { MenuItem } from "../models/MenuItem";
import { useAccount } from "./AccountProvider";

export const AccountHeader: React.FC<AccountHeaderProps> = (props) => {

    const account = useAccount();

    const { getMenuItems } = useRenderers(account);

    if (!account) return null;

    return (
        <PageHeader title={account.name} menuItems={getMenuItems()} breadcrumbs={[[account.name,`/accounts/${account.id}`]]} />
    )
}

AccountHeader.displayName = "AccountHeader";

const useRenderers = (account: Account) => {

    const getMenuItems = () => {

        if (!account) return [];

        const items: MenuItem[] = [{ route: `/accounts/${account.id}/tag-rules`, text: "Tag Rules" }];

        switch (account.controller) {
            case AccountController.Import:
                items.push({ route: `/accounts/${account.id}/import`, text: "Import" });
        }

        return items;
    };

    return {
        getMenuItems,
    };
}

export interface AccountHeaderProps {
}