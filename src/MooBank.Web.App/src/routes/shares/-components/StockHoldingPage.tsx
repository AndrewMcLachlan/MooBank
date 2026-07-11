import React from "react";
import type { PropsWithChildren, ReactNode } from "react";

import type { PageProps } from "@andrewmclachlan/moo-app";
import type { NavItem } from "@andrewmclachlan/moo-ds";
import { Reports, Sliders, Transaction } from "@andrewmclachlan/moo-icons";

import { InstrumentPage } from "components";
import { useStockHolding } from "./StockHoldingProvider";

export const StockHoldingPage: React.FC<PropsWithChildren<StockHoldingPageProps>> = ({ children, ...props }) => {

    const stockHolding = useStockHolding();

    const navItems: (NavItem | ReactNode)[] = [
        { route: `/shares/${stockHolding?.id}/transactions`, text: "Transactions", image: <Transaction /> },
        { route: `/shares/${stockHolding?.id}/reports`, text: "Reports", image: <Reports /> },
        { route: `/shares/${stockHolding?.id}/manage`, text: "Manage", image: <Sliders /> },
    ];

    return (
        <InstrumentPage instrument={stockHolding} instrumentRoute={`/shares/${stockHolding?.id}`} instrumentNavItems={navItems} {...props}>
            {children}
        </InstrumentPage>
    )
}

export interface StockHoldingPageProps extends PageProps {
}
