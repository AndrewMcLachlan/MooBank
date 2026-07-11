import React from "react";
import type { PropsWithChildren, ReactNode } from "react";

import { Page } from "@andrewmclachlan/moo-app";
import type { PageProps } from "@andrewmclachlan/moo-app";
import { NavItemDivider } from "@andrewmclachlan/moo-ds";
import type { NavItem } from "@andrewmclachlan/moo-ds";

export const InstrumentPage: React.FC<PropsWithChildren<InstrumentPageProps>> = ({ children, instrument, instrumentRoute, instrumentNavItems = [], breadcrumbs = [], ...props }) => {

    if (!instrument) return null;

    const navItems: (NavItem | ReactNode)[] = [...instrumentNavItems];

    const extraNavItems = props.navItems ?? [];
    if (extraNavItems.length > 0) {
        navItems.push(<NavItemDivider />);
    }

    return (
        <Page title={`${instrument.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={navItems.concat(extraNavItems)} breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: instrument.name, route: instrumentRoute }, ...breadcrumbs]}>
            {children}
        </Page>
    )
}

export interface InstrumentPageProps extends PageProps {
    instrument?: { id: string, name: string };
    instrumentRoute: string;
    instrumentNavItems?: (NavItem | ReactNode)[];
}
