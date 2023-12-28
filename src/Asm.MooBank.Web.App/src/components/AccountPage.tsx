import React, { PropsWithChildren, ReactNode } from "react";

import { InstitutionAccount, AccountController, VirtualAccount, isVirtualAccount } from "models";
import { useAccount } from "./AccountProvider";
import { NavItem, NavItemDivider, Page, PageProps } from "@andrewmclachlan/mooapp";

import { Import, Reports, Rules, Sliders, Transaction } from "assets";

export const AccountPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, ...props }) => {

    const account = useAccount();

    if (!account) return null;

    return (
        <Page title={`${account.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(account, props.navItems ?? [])} breadcrumbs={[...getBreadcrumbs(account), ...props.breadcrumbs]}>
            {children}
        </Page>
    )
}

AccountPage.displayName = "AccountPage";

AccountPage.defaultProps = {
    breadcrumbs: []
}

const getMenuItems = (account: InstitutionAccount | VirtualAccount, navItems: NavItem[]) => {

    if (!account) return [];

    const isVirtual = isVirtualAccount(account);

    const baseRoute = isVirtual ? `/accounts/${(account as VirtualAccount).parentId}/virtual/${account.id}` : `/accounts/${account.id}`;

    const items: (NavItem | ReactNode)[] = [
        { route: `${baseRoute}/transactions`, text: "Transactions", image: <Transaction /> },
    ];

    if (!isVirtual) {
        items.push({ route: `${baseRoute}/reports`, text: "Reports", image: <Reports /> });
        items.push({ route: `${baseRoute}/rules`, text: "Rules", image: <Rules /> });

        switch ((account as InstitutionAccount).controller) {
            case "Import":
                items.push({ route: `${baseRoute}/import`, text: "Import", image: <Import /> });
        }
    }
    
        items.push({ route: `${baseRoute}/manage`, text: "Manage", image: <Sliders /> });

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

const getBreadcrumbs = (account: InstitutionAccount | VirtualAccount): NavItem[] => {

    const isVirtual = isVirtualAccount(account);
    const route = isVirtual ? `/accounts/${(account as VirtualAccount).parentId}/virtual/${account.id}` : `/accounts/${account.id}`;

    return [
        { text: "Accounts", route: "/accounts" },
        { text: account.name, route: route },
    ];
}

export interface AccountPageProps extends PageProps {
}