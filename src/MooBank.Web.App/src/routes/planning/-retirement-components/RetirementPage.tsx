import React from "react";
import type { PropsWithChildren, ReactNode } from "react";
import { Page, type PageAction } from "@andrewmclachlan/moo-app";
import type { RetirementPlan } from "api/types.gen";
import { planningNavItems } from "../-components/planningNav";

export const RetirementPage: React.FC<PropsWithChildren<RetirementPageProps>> = ({ plan, actions, customActions, children, breadcrumbs = [] }) => (
    <Page
        title={plan?.name ?? "Retirement"}
        breadcrumbs={[{ text: "Planning", route: `/planning` }, { text: "Retirement", route: `/planning/retirement` }, ...breadcrumbs]}
        navItems={planningNavItems}
        actions={actions}
        customActions={customActions}
    >
        {children}
    </Page>
);

export interface RetirementPageProps {
    plan?: RetirementPlan;
    actions?: PageAction[];
    customActions?: ReactNode[];
    breadcrumbs?: { text: string; route: string }[];
}
