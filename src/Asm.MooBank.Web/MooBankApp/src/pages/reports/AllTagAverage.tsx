import React, { useRef, useState } from "react";

import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";


import { Page } from "../../layouts";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useAllTagAverageReport } from "../../services";

import { Bar, getElementAtEvent } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { useIdParams } from "../../hooks";
import { PeriodSelector } from "../../components/PeriodSelector";
import { Period, lastMonth } from "../../helpers/dateFns";
import { ReportType } from "../../models/reports";
import { ReportTypeSelector } from "../../components/ReportTypeSelector";
import { getCachedPeriod } from "../../helpers";
import { chartColours } from "./chartColours";
import { useNavigate } from "react-router";

ChartJS.register(...registerables);

export const AllTagAverage = () => {

    const { theme } = useLayout();
    const navigate = useNavigate();

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>(getCachedPeriod());

    const account = useAccount(accountId!);

    const report = useAllTagAverageReport(accountId!, period.startDate, period.endDate, reportType);

    const chartRef = useRef();

    const dataset: ChartData<"bar", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.amount) ?? [],
            backgroundColor: chartColours
        }],
    };
    //http://localhost:3005/accounts/6b4ae4d9-d4ba-41f7-80e6-076863df9407/reports/breakdown/29
    return (
        <Page title="All Tag Average">
            <ReportsHeader account={account.data} title="All Tag Average" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector value={period} onChange={setPeriod} />
                <section className="report">
                    <h3>Average Across Tags</h3>
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

const formatPeriod = (start: Date, end: Date): string => {

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}