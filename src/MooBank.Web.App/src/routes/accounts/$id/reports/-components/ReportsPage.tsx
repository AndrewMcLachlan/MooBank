import React, { useEffect } from "react";
import type { PropsWithChildren } from "react";

import type { ReportKind } from "api/types.gen";
import { useLocation, useNavigate } from "@tanstack/react-router";
import { AccountPage, useAccount } from "components";
import { getDefaultReportRoute, getReportNavItems } from "../-utils/reports";

export const ReportsPage: React.FC<PropsWithChildren<ReportsPageProps>> = ({ children, title, kind }) => {

    const account = useAccount();

    const location = useLocation();
    const navigate = useNavigate();

    const available = "availableReports" in account ? account.availableReports : undefined;
    const isAllowed = kind === undefined || !available || available.length === 0 || available.includes(kind);

    useEffect(() => {
        if (kind === undefined || !available || available.length === 0) return;
        if (!available.includes(kind)) {
            navigate({ to: getDefaultReportRoute(account.id, available), replace: true });
        }
    }, [kind, account.id, available, navigate]);

    if (!account) return null;
    if (!isAllowed) return null;

    return (
        <AccountPage
            navItems={getReportNavItems(account.id, available)}
            breadcrumbs={[{ text: "Reports", route: `/accounts/${account.id}/reports` }, { text: title, route: location.pathname }]}
            title={title}
        >
            {children}
        </AccountPage>
    );
};

ReportsPage.displayName = "ReportsPage";

export interface ReportsPageProps {
    title: string;
    kind?: ReportKind;
}
