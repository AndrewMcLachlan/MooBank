import React, { PropsWithChildren, } from "react";

import { Page, PageProps } from "@andrewmclachlan/mooapp";
import { TwoCoins } from "@andrewmclachlan/mooicons";

export const BudgetPage: React.FC<PropsWithChildren<BudgetPageProps>> = ({ children, breadcrumbs = [] }) => (

    <Page title="Budget" breadcrumbs={[{ text: "Budget", route: `/budget` }, ...breadcrumbs]} navItems={[{ text: "Report", route: `budget/report`, image: <TwoCoins /> }]}>
        {children}
    </Page>
);

export interface BudgetPageProps extends PageProps {
}
