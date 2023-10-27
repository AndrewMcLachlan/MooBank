import React, { useRef, useState } from "react";

import { useAccount } from "components";
import { ReportsPage } from "./ReportsPage";
import { useAllTagAverageReport } from "services";

import { Bar, getElementAtEvent } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { Section, useIdParams } from "@andrewmclachlan/mooapp";

import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { chartColours, desaturatedChartColours } from "../../helpers/chartColours";
import { useNavigate } from "react-router";

ChartJS.register(...registerables);

export const AllTagAverage = () => {

    const navigate = useNavigate();

    const account = useAccount();
    const accountId = useIdParams();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({ startDate: null, endDate: null });
    const [showGross] = useState<boolean>(false); //TODO: may make this an option

    const report = useAllTagAverageReport(accountId, period?.startDate, period?.endDate, reportType);

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
        <ReportsPage title="All Tag Average">
            <Section>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector onChange={setPeriod} />
            </Section>
            <Section className="report">
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
            </Section>
        </ReportsPage>
    );

}