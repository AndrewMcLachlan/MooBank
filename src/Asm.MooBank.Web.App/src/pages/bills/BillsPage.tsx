import React, { PropsWithChildren, ReactNode } from "react";
import { Page, PageProps } from "@andrewmclachlan/moo-app";
import { NavItem, NavItemDivider } from "@andrewmclachlan/moo-ds";
import { Users } from "@andrewmclachlan/mooicons";

export const BillsPage: React.FC<PropsWithChildren<BillsPageProps>> = ({ children, breadcrumbs = [], ...props }) => {

    return (
        <Page title={props.title} actions={props.actions} navItems={getMenuItems(props.navItems ?? [])} breadcrumbs={[{ text: "Bills", route: "/bills" }, ...breadcrumbs]}>
            {children}
        </Page>
    )
}

const getMenuItems = (navItems: (ReactNode | NavItem)[]) => {

    const items: (NavItem | ReactNode)[] = [
        { route: `/bills/accounts`, text: "Accounts", image: <Users /> },
    ];

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

export interface BillsPageProps extends PageProps {
}
