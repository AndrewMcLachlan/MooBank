import React from "react";
import type { PropsWithChildren, ReactNode } from "react";
import { Page, type PageAction } from "@andrewmclachlan/moo-app";
import type { ForecastPlan } from "api/types.gen";
import { planningNavItems } from "./planningNav";

export const ForecastPage: React.FC<PropsWithChildren<ForecastPageProps>> = ({ plan, actions, customActions, children, breadcrumbs = [] }) => (
    <Page
        title={plan?.name ?? "Forecast"}
        breadcrumbs={[{ text: "Planning", route: `/planning` }, ...breadcrumbs]}
        navItems={planningNavItems}
        actions={actions}
        customActions={customActions}
    >
        {children}
    </Page>
);

export interface ForecastPageProps {
    plan?: ForecastPlan;
    actions?: PageAction[];
    customActions?: ReactNode[];
    breadcrumbs?: { text: string; route: string }[];
}
