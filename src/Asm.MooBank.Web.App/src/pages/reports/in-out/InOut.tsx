import React from "react";

import { format } from "date-fns/format";
import { getMonth } from "date-fns/getMonth";
import { getYear } from "date-fns/getYear";

import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import { Bar } from "react-chartjs-2";

import { useChartColours } from "helpers/chartColours";
import { Period } from "helpers/dateFns";
import { useInOutReport } from "services";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const InOut: React.FC<InOutProps> = ({ accountId, period }) => {

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

    // Calculate the difference to the nearest $10
    const difference = Math.round((((report.data?.income ?? 0) - Math.abs(report.data?.outgoings ?? 0)) / 10.0)) * 10;

    if (!report.data) return null;

    return (
        <>
            {difference >= 0 ?
                <h4 className="text-success amount">${difference} saved</h4> :
                <h4 className="text-danger amount">${Math.abs(difference)} overspent</h4>
            }
            <Bar id="inout" data={dataset} options={{
                indexAxis: "y",
                maintainAspectRatio: false,
                scales: {
                    y: {
                        grid: {
                            display: false,
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
        </>
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
