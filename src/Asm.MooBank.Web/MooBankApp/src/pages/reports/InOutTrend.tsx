import React from "react";

import { useInOutTrendReport } from "../../services";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import { useLayout } from "@andrewmclachlan/mooapp";

import { useIdParams } from "../../hooks";
import { Period } from "../../helpers/dateFns";

ChartJS.register(...registerables);

export const InOutTrend: React.FC<InOutTrendProps> = ({period}) => {

    const { theme, defaultTheme } = useLayout();

    const theTheme = theme ?? defaultTheme;

    const accountId = useIdParams();

    const report = useInOutTrendReport(accountId!, period.startDate, period.endDate);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.income.map(i => i.month) ?? [],

        datasets: [{
            label: "Income",
            data: report.data?.income.map(i => i.amount) ?? [],
            backgroundColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            borderColor: theTheme === "dark" ? "#228b22" : "#00FF00",
            // @ts-ignore
            trendlineLinear: {
                colorMin: theTheme === "dark" ? "#99fb99" : "#bbfbbb",
                colorMax: theTheme === "dark" ? "#99fb99" : "#bbfbbb",
                lineStyle: "solid",
                width: 2,
            }
        }, {
            label: "Expenses",
            data: report.data?.expenses.map(i => Math.abs(i.amount)) ?? [],
            backgroundColor: theTheme === "dark" ? "#800020" : "#e23d28",
            borderColor: theTheme === "dark" ? "#800020" : "#e23d28",
            // @ts-ignore
            trendlineLinear: {
                colorMin: theTheme === "dark" ? "#FF7790" : "#FFAAC0",
                colorMax: theTheme === "dark" ? "#FF7790" : "#FFAAC0",
                lineStyle: "solid",
                width: 2,
            }
        }]
    };

    return (
        <section className="report">
            <h3>Income vs Expenses per Month</h3>
            <Line id="inout" data={dataset} options={{
                maintainAspectRatio: true,
                scales: {
                    y: {
                        suggestedMin: 0,
                        ticks: {
                            stepSize: 5000,
                        },
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
    );
}

export interface InOutTrendProps {
    period: Period;
}