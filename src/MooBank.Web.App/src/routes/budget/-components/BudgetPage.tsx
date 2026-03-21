import React from "react";
import type { PropsWithChildren } from "react";

import { Page } from "@andrewmclachlan/moo-app";
import type { PageProps } from "@andrewmclachlan/moo-app";
import { TwoCoins } from "@andrewmclachlan/moo-icons";

export const BudgetPage: React.FC<PropsWithChildren<BudgetPageProps>> = ({ children, breadcrumbs = [] }) => (

    <Page title="Budget" breadcrumbs={[{ text: "Budget", route: `/budget` }, ...breadcrumbs]} navItems={[{ text: "Report", route: `budget/report`, image: <TwoCoins /> }]}>
        {children}
    </Page>
);

export interface BudgetPageProps extends PageProps {
}
