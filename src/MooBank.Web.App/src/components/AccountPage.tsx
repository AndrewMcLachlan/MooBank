import React from "react";
import type { PropsWithChildren, ReactNode } from "react";

import { Page } from "@andrewmclachlan/moo-app";
import type { PageProps } from "@andrewmclachlan/moo-app";
import { NavItemDivider } from "@andrewmclachlan/moo-ds";
import type { NavItem } from "@andrewmclachlan/moo-ds";
import { Import, Reports, Rules, Sliders, Transaction } from "@andrewmclachlan/moo-icons";

import type { LogicalAccount, VirtualInstrument } from "api/types.gen";
import { isVirtualInstrument } from "utils/virtualAccounts";
import { useAccount } from "./AccountProvider";

export const AccountPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, breadcrumbs = [], ...props }) => {

    const account = useAccount();

    if (!account) return null;

    return (
        <Page title={`${account.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(account, props.navItems ?? [])} breadcrumbs={[...getBreadcrumbs(account), ...breadcrumbs]}>
            {children}
        </Page>
    )
}

const getMenuItems = (account: LogicalAccount | VirtualInstrument, navItems: (ReactNode | NavItem)[]) => {

    if (!account) return [];

    const isVirtual = isVirtualInstrument(account);

    const baseRoute = isVirtual ? `/accounts/${(account as VirtualInstrument).parentId}/virtual/${account.id}` : `/accounts/${account.id}`;

    const items: (NavItem | ReactNode)[] = [
        { route: `${baseRoute}/transactions`, text: "Transactions", image: <Transaction /> },
    ];

    if (!isVirtual) {
        items.push({ route: `${baseRoute}/reports`, text: "Reports", image: <Reports /> });
        items.push({ route: `${baseRoute}/rules`, text: "Rules", image: <Rules /> });
    }

    items.push({ route: `${baseRoute}/manage`, text: "Manage", image: <Sliders /> });

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

const getBreadcrumbs = (account: LogicalAccount | VirtualInstrument): NavItem[] => {

    const isVirtual = isVirtualInstrument(account);
    const route = isVirtual ? `/accounts/${(account as VirtualInstrument).parentId}/virtual/${account.id}` : `/accounts/${account.id}`;

    return [
        { text: "Accounts", route: "/accounts" },
        { text: account.name, route: route },
    ];
}

export interface AccountPageProps extends PageProps {
}
