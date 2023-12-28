import React, { useState } from "react";

import { format } from "date-fns/format";
import { getMonth } from "date-fns/getMonth";
import { getYear } from "date-fns/getYear";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { Period } from "helpers/dateFns";
import { useInOutReport } from "services";
import { useChartColours } from "helpers/chartColours";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const InOut: React.FC<InOutProps> = ({accountId, period}) => {

    const colours = useChartColours();

    const report = useInOutReport(accountId!, period?.startDate, period?.endDate);

    const dataset: ChartData<"bar", number[], string> = {
        labels: [formatPeriod(period?.startDate, period?.endDate)],

        datasets: [{
            label: "Income",
            data: [report.data?.income ?? 0],
            backgroundColor: colours.income,
        }, {
            label: "Expenses",
            data: [Math.abs(report.data?.outgoings ?? 0)],
            backgroundColor: colours.expenses,
        }]
    };

    return (
        <Bar id="inout" data={dataset} options={{
            indexAxis: "y",
            maintainAspectRatio: false,
            scales: {
                y: {
                    grid: {
                        color: colours.grid,
                    },
                },
                x: {
                    grid: {
                        color: colours.grid,
                    },
                }
            }
        }} />
    );

}

const formatPeriod = (start?: Date, end?: Date): string => {

    if (!start && !end) return "All time";

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}

export interface InOutProps {
    accountId: string;
    period: Period;
}