import { useRef, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { byTagReportOptions } from "api/@tanstack/react-query.gen";
import { warmReport } from "./-utils/warmReport";

import { ReportsPage } from "./-components/ReportsPage";
import { useByTagReport } from "../../-hooks/useByTagReport";

import { Doughnut } from "react-chartjs-2";
import type { ChartData } from "chart.js";
import { useIdParams } from "@andrewmclachlan/moo-app";
import { Section} from "@andrewmclachlan/moo-ds";

import type { Period } from "models/dateFns";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { chartColours } from "utils/chartColours";
import type { transactionTypeFilter } from "models/transactions";
import { DateRangeSelector } from "components/DateRangeSelector";
import { getDateRange } from "hooks";


export const Route = createFileRoute("/accounts/$id/reports/by-tag")({
    // Matches the page default reportType "Debit".
    loader: ({ params }) => warmReport(params.id, ({ accountId, start, end }) => byTagReportOptions({ path: { accountId, start, end, reportType: "debit" as any } })),
    component: ByTag,
});

function ByTag() {

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getDateRange());

    const report = useByTagReport(accountId!, period?.startDate, period?.endDate, reportType);

    const chartRef = useRef(null);

    const dataset: ChartData<"doughnut", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.grossAmount) ?? [],
            backgroundColor: chartColours,
            borderRadius: 10,
            spacing: 10,
            borderColor: "transparent",
            //categoryPercentage: 1,
        }],
    };

    return (
        <ReportsPage title="All Tags" kind="AllTags">
            <Section className="mini-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <DateRangeSelector onChange={setPeriod} />
            </Section>
            <section className="report doughnut">
                <h3>All Tags</h3>
                <Doughnut id="bytag" ref={chartRef} data={dataset} options={{
                    plugins: {
                        legend: {
                            position: "right"
                        },
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    hover: {
                        mode: "point",
                        intersect: true,
                    },
                }}
                />
            </section>
        </ReportsPage>
    );
}
