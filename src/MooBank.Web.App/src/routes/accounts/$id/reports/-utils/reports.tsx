import type { NavItem } from "@andrewmclachlan/moo-ds";
import { BarChart, LeftRightArrow, PieChart, Tags, Trendline } from "@andrewmclachlan/moo-icons";
import type { ReportKind } from "api/types.gen";

interface ReportDescriptor {
    text: string;
    image: React.ReactNode;
    path: string;
}

const descriptors: Partial<Record<ReportKind, ReportDescriptor>> = {
    InOut: { text: "Income vs Expenses", image: <LeftRightArrow />, path: "in-out" },
    TopTags: { text: "Top Tags", image: <BarChart />, path: "all-tag-average" },
    Breakdown: { text: "Breakdown", image: <PieChart />, path: "breakdown" },
    TagTrend: { text: "Tag Trend", image: <Trendline />, path: "tag-trend" },
    AllTags: { text: "All Tags", image: <Tags />, path: "by-tag" },
    MonthlyBalances: { text: "Monthly Balances", image: <Trendline />, path: "monthly-balances" },
    SavingsInterest: { text: "Interest Received", image: <Trendline />, path: "savings-interest" },
    SuperContributions: { text: "Contributions", image: <BarChart />, path: "super-contributions" },
    SuperReturns: { text: "Annual Returns", image: <Trendline />, path: "super-returns" },
    PrincipalVsInterest: { text: "Principal vs Interest", image: <BarChart />, path: "principal-vs-interest" },
};

export const getReportNavItems = (accountId: string, availableReports: ReportKind[] | undefined): NavItem[] => {
    if (!availableReports) return [];
    return availableReports
        .map(kind => descriptors[kind])
        .filter((d): d is ReportDescriptor => d !== undefined)
        .map(d => ({
            text: d.text,
            image: d.image,
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
