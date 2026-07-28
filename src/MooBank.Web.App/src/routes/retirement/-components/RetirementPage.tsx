import React from "react";
import type { PropsWithChildren, ReactNode } from "react";
import { Page } from "@andrewmclachlan/moo-app";
import type { RetirementPlan } from "api/types.gen";

export const RetirementPage: React.FC<PropsWithChildren<RetirementPageProps>> = ({ plan, actions, children, breadcrumbs = [] }) => (
    <Page
        title={plan?.name ?? "Retirement"}
        breadcrumbs={[{ text: "Retirement", route: `/retirement` }, ...breadcrumbs]}
        actions={actions}
    >
        {children}
    </Page>
);

export interface RetirementPageProps {
    plan?: RetirementPlan;
    actions?: ReactNode[];
    breadcrumbs?: { text: string; route: string }[];
}
