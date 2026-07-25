import React from "react";
import type { PropsWithChildren, ReactNode } from "react";
import { Page } from "@andrewmclachlan/moo-app";
import type { ForecastPlan } from "api/types.gen";

export const ForecastPage: React.FC<PropsWithChildren<ForecastPageProps>> = ({ plan, actions, children, breadcrumbs = [] }) => (
    <Page
        title={plan?.name ?? "Forecast"}
        breadcrumbs={[{ text: "Forecast", route: `/forecast` }, ...breadcrumbs]}
        actions={actions}
    >
        {children}
    </Page>
);

export interface ForecastPageProps {
    plan?: ForecastPlan;
    actions?: ReactNode[];
    breadcrumbs?: { text: string; route: string }[];
}
