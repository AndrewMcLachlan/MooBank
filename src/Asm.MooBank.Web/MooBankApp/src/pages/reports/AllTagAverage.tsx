import React, { useRef, useState } from "react";

import { Page } from "layouts";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useAllTagAverageReport } from "services";

import { Bar, getElementAtEvent } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useIdParams, useLayout } from "@andrewmclachlan/mooapp";

import { PeriodSelector } from "components/PeriodSelector";
import { Period, lastMonth } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { getCachedPeriod } from "helpers";
import { chartColours, desaturatedChartColours } from "./chartColours";
import { useNavigate } from "react-router";

ChartJS.register(...registerables);

export const AllTagAverage = () => {

    const { theme } = useLayout();
    const navigate = useNavigate();

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({startDate: null,endDate: null});
    const [showGross] = useState<boolean>(false); //TODO: may make this an option

    const account = useAccount(accountId!);

    const report = useAllTagAverageReport(accountId!, period?.startDate, period?.endDate, reportType);

    const chartRef = useRef();

    const dataset: ChartData<"bar", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.netAmount!) ?? [],
            backgroundColor: chartColours,
        }],
    };

    if (showGross) {
        dataset.datasets.push({
            label: "",
            data: report.data?.tags.map(t => t.grossAmount!) ?? [],
            backgroundColor: desaturatedChartColours,
        });
    }

    return (
        <Page title="All Tag Average">
            <ReportsHeader account={account.data} title="All Tag Average" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector onChange={setPeriod} />
                <section className="report">
                    <h3>Average Across Top 50 Tags</h3>
                    <Bar id="alltagaverage" ref={chartRef} data={dataset} options={{
                        plugins: {
                            legend: {
                                display: false,
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
                        scales: {
                            x: {
                                stacked: true
                            }
                        }
                    }}
                        onClick={(e) => {
                            var elements = getElementAtEvent(chartRef.current!, e);
                            if (elements.length !== 1) return;
                            if (!report.data!.tags[elements[0].index].hasChildren) return;
                            navigate(`/accounts/${accountId}/reports/breakdown/${report.data!.tags[elements[0].index].tagId}`);
                        }}
                     />
                </section>
            </Page.Content>
        </Page >
    );

}
