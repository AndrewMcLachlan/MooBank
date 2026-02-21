import { useRef, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";

import { ReportsPage } from "./-components/ReportsPage";
import { useByTagReport } from "services";

import { Doughnut } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useIdParams } from "@andrewmclachlan/moo-app";
import { Section} from "@andrewmclachlan/moo-ds";

import { Period } from "helpers/dateFns";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { chartColours } from "helpers/chartColours";
import { transactionTypeFilter } from "store/state";
import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { getPeriod } from "hooks";

ChartJS.register(...registerables);

export const Route = createFileRoute("/accounts/$id/reports/by-tag")({
    component: ByTag,
});

function ByTag() {

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<transactionTypeFilter>("Debit");
    const [period, setPeriod] = useState<Period>(getPeriod());

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
        <ReportsPage title="All Tags">
            <Section className="mini-filter-panel">
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <MiniPeriodSelector value={period} onChange={setPeriod} />
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
