import type { IconType, NavItem } from "@andrewmclachlan/moo-ds";
import { BarChart, LeftRightArrow, PieChart, Tags, Trendline } from "@andrewmclachlan/moo-icons";
import type { ReportKind } from "api/types.gen";

interface ReportDescriptor {
    text: string;
    icon: IconType;
    path: string;
}

const descriptors: Partial<Record<ReportKind, ReportDescriptor>> = {
    InOut: { text: "Income vs Expenses", icon: LeftRightArrow, path: "in-out" },
    TopTags: { text: "Top Tags", icon: BarChart, path: "all-tag-average" },
    Breakdown: { text: "Breakdown", icon: PieChart, path: "breakdown" },
    TagTrend: { text: "Tag Trend", icon: Trendline, path: "tag-trend" },
    AllTags: { text: "All Tags", icon: Tags, path: "by-tag" },
    MonthlyBalances: { text: "Monthly Balances", icon: Trendline, path: "monthly-balances" },
    SavingsInterest: { text: "Interest Received", icon: Trendline, path: "savings-interest" },
    SuperContributions: { text: "Contributions", icon: BarChart, path: "super-contributions" },
    SuperReturns: { text: "Annual Returns", icon: Trendline, path: "super-returns" },
    PrincipalVsInterest: { text: "Principal vs Interest", icon: BarChart, path: "principal-vs-interest" },
};

export const getReportNavItems = (accountId: string, availableReports: ReportKind[] | undefined): NavItem[] => {
    if (!availableReports) return [];
    return availableReports
        .map(kind => descriptors[kind])
        .filter((d): d is ReportDescriptor => d !== undefined)
        .map(d => ({
            text: d.text,
            icon: d.icon,
            route: `/accounts/${accountId}/reports/${d.path}`,
        }));
};

export const getReportRoute = (accountId: string, kind: ReportKind): string | undefined => {
    const d = descriptors[kind];
    return d ? `/accounts/${accountId}/reports/${d.path}` : undefined;
};

export const getDefaultReportRoute = (accountId: string, availableReports: ReportKind[] | undefined): string => {
    const first = availableReports?.find(k => descriptors[k]);
    return first ? `/accounts/${accountId}/reports/${descriptors[first]!.path}` : `/accounts/${accountId}/reports/in-out`;
};
