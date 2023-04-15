import React, { useState } from "react";

import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";

import { Page } from "layouts";
import { ReportsHeader } from "./ReportsHeader";
import { useAccount, useInOutReport } from "services";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import { useLayout } from "@andrewmclachlan/mooapp";

import { PeriodSelector } from "components/PeriodSelector";
import { useIdParams } from "hooks";
import { getCachedPeriod } from "helpers";
import { InOutTrend } from "./InOutTrend";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const InOut = () => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const accountId= useIdParams();

    const [period, setPeriod] = useState(getCachedPeriod());

    const account = useAccount(accountId!);


    const report = useInOutReport(accountId!, period.startDate, period.endDate);

    const dataset: ChartData<"bar", number[], string> = {
        labels: [formatPeriod(period.startDate, period.endDate)],

        datasets: [{
            label: "Income",
            data: [report.data?.income ?? 0],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
        }, {
            label: "Expenses",
            data: [Math.abs(report.data?.outgoings ?? 0)],
            backgroundColor: theTheme === "dark" ? "#800020" : "#e23d28",
        }]
    };

    return (
        <Page title="Income vs Expenses">
            <ReportsHeader account={account.data} title="Income vs Expenses" />
            <Page.Content>
             <PeriodSelector value={period} onChange={setPeriod} />
                <section className="report inout">
                    <h3>Total Income vs Expenses</h3>
                    <Bar id="inout" data={dataset} options={{
                        indexAxis: "y",
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                grid: {
                                    color: theTheme === "dark" ? "#333" : "#E5E5E5"
                                },
                            },
                            x: {
                                grid: {
                                    color: theTheme === "dark" ? "#333" : "#E5E5E5"
                                },
                            }
                        }
                    }} />
                </section>
                <InOutTrend period={period} />
            </Page.Content>
        </Page >
    );

}

interface PeriodOption {
    value: string,
    label: string,
    start?: Date,
    end?: Date,
}

const formatPeriod = (start: Date, end: Date): string => {

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}