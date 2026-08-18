import { Section } from "@andrewmclachlan/moo-ds";
import type { ChartData } from "chart.js";
import React, { useState } from "react";
import { Line } from "react-chartjs-2";
import { useParams } from "@tanstack/react-router";

import { DateRangeSelector } from "components/DateRangeSelector";
import type { Period } from "models/dateFns";
import { getDateRange } from "hooks";
import { useMonthlyBalancesReport } from "../../../-hooks/useMonthlyBalancesReport";
import { useChartColours } from "utils/chartColours";
import { getStepSize } from "utils/charts";
import { ReportsPage } from "./ReportsPage";


export const MonthlyBalances: React.FC = () => {

    const colours = useChartColours();

    const { id: accountId  } = useParams({ strict: false });

    const [period, setPeriod] = useState<Period>(getDateRange());
    const report = useMonthlyBalancesReport(accountId!, period?.startDate, period?.endDate);

        const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.balances.map(i => i.month) ?? [],

        datasets: [{
            label: "End of Month Balance",
            data: report.data?.balances.map(i => i.grossAmount) ?? [],
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
        <ReportsPage title="Monthly Balances" kind="MonthlyBalances">
            <Section className="mini-filter-panel">
                <DateRangeSelector onChange={setPeriod} />
            </Section>
            <Section className="report" header="Monthly Balances" headerSize={3}>
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
