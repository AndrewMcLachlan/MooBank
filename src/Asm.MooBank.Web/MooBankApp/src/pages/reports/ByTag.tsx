import React, { useRef, useState } from "react";

import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";


import { Page } from "layouts";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useByTagReport, useTag } from "services";

import { Doughnut } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { useIdParams } from "hooks";
import { PeriodSelector } from "components/PeriodSelector";
import { Period } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { ReportTypeSelector } from "components/ReportTypeSelector";
import { getCachedPeriod } from "helpers";
import { chartColours } from "./chartColours";

ChartJS.register(...registerables);


export const ByTag = () => {


    const { theme, defaultTheme } = useLayout();
    const theTheme = theme ?? defaultTheme;

    const accountId = useIdParams();

    const [reportType, setReportType] = useState<ReportType>(ReportType.Expenses);
    const [period, setPeriod] = useState<Period>({startDate: null,endDate: null});

    const account = useAccount(accountId!);

    const report = useByTagReport(accountId!, period?.startDate, period?.endDate, reportType);

    const chartRef = useRef();

    const dataset: ChartData<"doughnut", number[], string> = {
        labels: report.data?.tags.map(t => t.tagName) ?? [],
        datasets: [{
            label: "",
            data: report.data?.tags.map(t => t.grossAmount) ?? [],
            backgroundColor: chartColours,//theTheme === "dark" ? "#228b22" : "#00FF00",
            //categoryPercentage: 1,
        }],
    };

    return (
        <Page title="By Tag">
            <ReportsHeader account={account.data} title="By Tag" />
            <Page.Content>
                <ReportTypeSelector value={reportType} onChange={setReportType} hidden />
                <PeriodSelector value={period} onChange={setPeriod} />
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