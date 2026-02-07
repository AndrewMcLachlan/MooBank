import React, { PropsWithChildren, ReactNode } from "react";

import { Page, PageProps } from "@andrewmclachlan/moo-app";
import { NavItem, NavItemDivider } from "@andrewmclachlan/moo-ds";
import { StockHolding } from "models";
import { useStockHolding } from "./StockHoldingProvider";

import { Reports, Sliders, Transaction } from "@andrewmclachlan/moo-icons";

export const StockHoldingPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, breadcrumbs = [], ...props }) => {

    const stockHolding = useStockHolding();

    if (!stockHolding) return null;

    return (
        <Page title={`${stockHolding.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(stockHolding, props.navItems ?? [])} breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: stockHolding.name, route: `/shares/${stockHolding.id}` }, ...breadcrumbs]}>
            {children}
        </Page>
    )
}

const getMenuItems = (stockHolding: StockHolding, navItems: (ReactNode | NavItem)[]) => {

    if (!stockHolding) return [];

    const items: (NavItem | ReactNode)[] = [
        { route: `/shares/${stockHolding.id}/transactions`, text: "Transactions", image: <Transaction /> },
    ];

    items.push({ route: `/shares/${stockHolding.id}/reports`, text: "Reports", image: <Reports /> });
    items.push({ route: `/shares/${stockHolding.id}/manage`, text: "Manage", image: <Sliders /> });

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

export interface AccountPageProps extends PageProps {
}
