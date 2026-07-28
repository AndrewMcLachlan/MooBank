import React from "react";
import type { PropsWithChildren, ReactNode } from "react";
import { Page } from "@andrewmclachlan/moo-app";
import type { ForecastPlan } from "api/types.gen";
import { PlanningTabs } from "./PlanningTabs";

export const ForecastPage: React.FC<PropsWithChildren<ForecastPageProps>> = ({ plan, actions, children, breadcrumbs = [] }) => (
    <Page
        title={plan?.name ?? "Forecast"}
        breadcrumbs={[{ text: "Planning", route: `/planning` }, ...breadcrumbs]}
        actions={actions}
    >
        <PlanningTabs active="forecast" />
        {children}
    </Page>
);

export interface ForecastPageProps {
    plan?: ForecastPlan;
    actions?: ReactNode[];
    breadcrumbs?: { text: string; route: string }[];
}
