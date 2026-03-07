import React from "react";

import { format } from "date-fns/format";
import { getMonth } from "date-fns/getMonth";
import { getYear } from "date-fns/getYear";

import { ChartData } from "chart.js";
import { Bar } from "react-chartjs-2";

import { useChartColours } from "utils/chartColours";
import type { Period } from "models/dateFns";
import { useInOutReport as defaultReport } from "hooks/useInOutReport";
import { SpinnerContainer } from "@andrewmclachlan/moo-ds";


export const InOut: React.FC<InOutProps> = ({ accountId, period, useInOutReport = defaultReport }) => {
defaultReport
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

    return (
        <>
            {difference >= 0 ?
                <h4 className="text-success amount">${difference} saved</h4> :
                <h4 className="text-danger amount">${Math.abs(difference)} overspent</h4>
            }
            { report.isLoading && <SpinnerContainer />}
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
    useInOutReport: (accountId: string, startDate?: Date, endDate?: Date) => any;
}
