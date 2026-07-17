import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { allTagAverageReportOptions } from "api/@tanstack/react-query.gen";
import { warmReport } from "./-utils/warmReport";

import { ReportsPage } from "./-components/ReportsPage";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { Section } from "@andrewmclachlan/moo-ds";


import { ReportTypeSelector } from "components/ReportTypeSelector";
import type { Period } from "models/dateFns";
import { subtractYear } from "utils/dateFns";
import { TopTags } from "./-components/TopTags";
import type { transactionTypeFilter } from "models/transactions";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";
import { differenceInMonths } from "date-fns";


export const Route = createFileRoute("/accounts/$id/reports/all-tag-average")({
    // Matches the page defaults: reportType "Debit", top 20, monthly.
    loader: ({ params }) => warmReport(params.id, ({ accountId, start, end }) => allTagAverageReportOptions({ path: { accountId, start, end, reportType: "debit" as any }, query: { Top: 20, Interval: "Monthly" } })),
    component: AllTagAverage,
});

function AllTagAverage() {

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getPeriod());

const difference = Math.abs(differenceInMonths(period.startDate, period.endDate));

    return (
        <ReportsPage title="Top Tags" kind="TopTags">
            <Section className="mini-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <MiniPeriodSelector onChange={setPeriod} />
            </Section>
            <Section className="report">
                <h3>Average per month Across Top 20 Tags</h3>
                <TopTags accountId={accountId} period={period} reportType={reportType} />
            </Section>
            <Section className="report" hidden={difference > 12}>
                <h3>Same Period Last Year</h3>
                <TopTags accountId={accountId} period={subtractYear(period)} reportType={reportType} />
            </Section>
        </ReportsPage>
    );
}
