import React, { PropsWithChildren, ReactNode } from "react";

import { Account, StockHolding } from "models";
import { useStockHolding } from "./StockHoldingProvider";
import { NavItem, NavItemDivider, Page, PageProps } from "@andrewmclachlan/mooapp";

import { Import, Reports, Rules, Sliders, Transaction } from "assets";

export const StockHoldingPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, ...props }) => {

    const stockHolding = useStockHolding();

    if (!stockHolding) return null;

    return (
        <Page title={`${stockHolding.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(stockHolding, props.navItems ?? [])} breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: stockHolding.name, route: `/accounts/${stockHolding.id}` }, ...props.breadcrumbs]}>
            {children}
        </Page>
    )
}

StockHoldingPage.displayName = "StockHoldingPage";

StockHoldingPage.defaultProps = {
    breadcrumbs: []
}

const getMenuItems = (stockHolding: StockHolding, navItems: NavItem[]) => {

    if (!stockHolding) return [];

    const items: (NavItem | ReactNode)[] = [
        { route: `/stock/${stockHolding.id}/transactions`, text: "Transactions", image: <Transaction /> },
    ];

    items.push({ route: `/stock/${stockHolding.id}/manage`, text: "Manage", image: <Sliders /> });

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

export interface AccountPageProps extends PageProps {
}