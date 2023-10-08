import React, { PropsWithChildren, } from "react";

import { Page, PageProps } from "@andrewmclachlan/mooapp";

export const BudgetPage: React.FC<PropsWithChildren<BudgetPageProps>> = ({ children, ...props }) => (

<Page title="Budget" breadcrumbs={[{ text: "Budget", route: `/budget` }, ...props.breadcrumbs]} navItems={[{ text: "Report", route: `budget/report`}]}>
    {children}
</Page>
);

BudgetPage.displayName = "BudgetPage";

BudgetPage.defaultProps = {
    breadcrumbs: []
}

export interface BudgetPageProps extends PageProps {
}