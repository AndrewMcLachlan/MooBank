import { useState } from "react";

import { ReportsPage } from "../ReportsPage";

import { Section, useIdParams } from "@andrewmclachlan/mooapp";
import { Chart as ChartJS, registerables } from "chart.js";

import { PeriodSelector } from "components/PeriodSelector";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { Period, subtractYear } from "helpers/dateFns";
import { TopTags } from "./TopTags";
import { transactionTypeFilter } from "store/state";

ChartJS.register(...registerables);

export const AllTagAverage = () => {

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    return (
        <ReportsPage title="Top Tags">
            <Section>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector onChange={setPeriod} />
            </Section>
            <Section className="report">
                <h3>Average per month Across Top 20 Tags</h3>
                <TopTags accountId={accountId} period={period} reportType={reportType} />
            </Section>
            <Section className="report">
                <h3>Same Period Last Year</h3>
                <TopTags accountId={accountId} period={subtractYear(period)} reportType={reportType} />
            </Section>
        </ReportsPage>
    );

}
