import React from "react";
import type { PropsWithChildren } from "react";
import { Page } from "@andrewmclachlan/moo-app";

export const ForecastPage: React.FC<PropsWithChildren<ForecastPageProps>> = ({ children, breadcrumbs = [] }) => (
    <Page title="Forecast" breadcrumbs={[{ text: "Forecast", route: `/forecast` }, ...breadcrumbs]}>
        {children}
    </Page>
);

export interface ForecastPageProps {
    breadcrumbs?: { text: string; route: string }[];
}
