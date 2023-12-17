import React, { PropsWithChildren, ReactNode } from "react";

import { Account, AccountController } from "models";
import { useAccount } from "./AccountProvider";
import { NavItem, NavItemDivider, Page, PageProps } from "@andrewmclachlan/mooapp";

import { Import, Reports, Rules, Sliders, Transaction } from "assets";

export const AccountPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, ...props }) => {

    const account = useAccount();

    if (!account) return null;

    return (
        <Page title={`${account.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(account, props.navItems ?? [])} breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: account.name, route: `/accounts/${account.id}` }, ...props.breadcrumbs]}>
            {children}
        </Page>
    )
}

AccountPage.displayName = "AccountPage";

AccountPage.defaultProps = {
    breadcrumbs: []
}

const getMenuItems = (account: Account, navItems: NavItem[]) => {

    if (!account) return [];

    const items: (NavItem | ReactNode)[] = [
        { route: `/accounts/${account.id}/transactions`, text: "Transactions", image: <Transaction /> },
    ];

    if (account.accountType) {
        items.push({ route: `/accounts/${account.id}/reports`, text: "Reports", image: <Reports /> });
        items.push({ route: `/accounts/${account.id}/rules`, text: "Rules", image: <Rules /> });
    }
    
    switch (account.controller) {
        case "Import":
            items.push({ route: `/accounts/${account.id}/import`, text: "Import", image: <Import /> });
    }

    items.push({ route: `/accounts/${account.id}/manage`, text: "Manage", image: <Sliders /> });

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

export interface AccountPageProps extends PageProps {
}