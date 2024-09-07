import React, { PropsWithChildren } from "react";

import { InstitutionAccount, StockHolding } from "models";
import { useLocation } from "react-router";
import { NavItem } from "@andrewmclachlan/mooapp";
import { AccountPage, useAccount } from "components";
import { Trendline } from "assets";
import { useStockHolding } from "../StockHoldingProvider";
import { StockHoldingPage } from "../StockHoldingPage";

export const ReportsPage: React.FC<PropsWithChildren<ReportsHeaderProps>> = ({ children, title }) => {

    const account = useStockHolding();

    const location = useLocation();

    if (!account) return  null;

    return (
        <StockHoldingPage navItems={getMenuItems(account as StockHolding)} breadcrumbs={[{ text: "Reports", route: `/accounts/${account.id}/reports` }, { text: title, route: location.pathname }]} title={title}>
            {children}
        </StockHoldingPage>
    )
}

ReportsPage.displayName = "ReportsHeader";

const getMenuItems = (account: StockHolding) => {

    if (!account) return [];

    const items: NavItem[] = [
        { route: `/shares/${account.id}/reports/value`, text: "Value Trend", image: <Trendline/> },
    ];

    return items;
};

export interface ReportsHeaderProps {
    title: string;
}
