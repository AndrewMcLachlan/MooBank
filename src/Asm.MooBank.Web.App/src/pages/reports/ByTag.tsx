import { useRef, useState } from "react";

import { ReportsPage } from "./ReportsPage";
import { useByTagReport } from "services";

import { Doughnut } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { Section, useIdParams } from "@andrewmclachlan/mooapp";

import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { chartColours } from "../../helpers/chartColours";

ChartJS.register(...registerables);

export const ByTag = () => {

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });

    const report = useByTagReport(accountId!, period?.startDate, period?.endDate, reportType);

    const chartRef = useRef();

    const dataset: ChartData<"doughnut", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.grossAmount) ?? [],
            backgroundColor: chartColours,
            //categoryPercentage: 1,
        }],
    };

    return (
        <ReportsPage title="By Tag">
            <Section>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector value={period} onChange={setPeriod} />
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
