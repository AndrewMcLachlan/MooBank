import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";

import { ReportsPage } from "./-components/ReportsPage";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { Section } from "@andrewmclachlan/moo-ds";
import { Chart as ChartJS, registerables } from "chart.js";

import { ReportTypeSelector } from "components/ReportTypeSelector";
import type { Period } from "models/dateFns";
import { subtractYear } from "utils/dateFns";
import { TopTags } from "./-components/TopTags";
import { transactionTypeFilter } from "store/state";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";
import { differenceInMonths } from "date-fns";

ChartJS.register(...registerables);

export const Route = createFileRoute("/accounts/$id/reports/all-tag-average")({
    component: AllTagAverage,
});

function AllTagAverage() {

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getPeriod());

const difference = Math.abs(differenceInMonths(period.startDate, period.endDate));

    return (
        <ReportsPage title="Top Tags">
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
