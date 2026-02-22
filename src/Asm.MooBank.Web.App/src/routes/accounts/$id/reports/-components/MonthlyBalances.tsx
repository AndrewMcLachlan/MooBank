import { Section } from "@andrewmclachlan/moo-ds";
import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import React, { useState } from "react";
import { Line } from "react-chartjs-2";
import { useParams } from "@tanstack/react-router";

import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import type { Period } from "models/dateFns";
import { getPeriod } from "hooks";
import { useMonthlyBalancesReport } from "../../../-hooks/useMonthlyBalancesReport";
import { useChartColours } from "utils/chartColours";
import { ReportsPage } from "./ReportsPage";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const MonthlyBalances: React.FC = () => {

    const colours = useChartColours();

    const { id: accountId  } = useParams({ strict: false });

    const [period, setPeriod] = useState<Period>(getPeriod());
    const report = useMonthlyBalancesReport(accountId!, period?.startDate, period?.endDate);

        const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.balances.map(i => i.month) ?? [],

        datasets: [{
            label: "End of Month Balance",
            data: report.data?.balances.map(i => Math.abs(i.grossAmount)) ?? [],
            backgroundColor: colours.income,
            borderColor: colours.income,
            // @ts-expect-error Not a known property for some reason
            trendlineLinear: {
                colorMin: colours.incomeTrend,
                colorMax: colours.incomeTrend,
                lineStyle: "solid",
                width: 2,
            }
        }]
    };

    return (
        <ReportsPage title="Tag Trend">
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} instant />
            </Section>
            <Section className="report" header="Tag Trend" headerSize={3}>
                <Line id="inout" data={dataset} options={{
                    maintainAspectRatio: true,
                    scales: {
                        y: {
                            suggestedMin: 0,
                            ticks: {
                                stepSize: getStepSize(dataset.datasets[0].data),
                            },
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
            </Section>
        </ReportsPage >
    );
}

const getStepSize = (values: number[]) => {
    const max = values.reduce((max, d) => d > max ? d : max, -Infinity);
    const magnitude = Math.pow(10, Math.floor(Math.log10(max)));
    const roundedMax = Math.ceil(max / magnitude) * magnitude;
    return roundedMax / 10;
}
