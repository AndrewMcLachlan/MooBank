import { Page } from "@andrewmclachlan/moo-app";
import { Section } from "@andrewmclachlan/moo-ds";
import { ChartData, Chart as ChartJS, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";
import React, { useState } from "react";
import { Line } from "react-chartjs-2";
import { useParams } from "@tanstack/react-router";

import { MiniPeriodSelector } from "components/MiniPeriodSelector";
import { Period } from "helpers/dateFns";
import { getPeriod } from "hooks";
import { useGroupMonthlyBalancesReport } from "../-hooks/useGroupMonthlyBalancesReport";
import { useGroup } from "../-hooks/useGroup";
import { useChartColours } from "helpers/chartColours";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const GroupMonthlyBalances: React.FC = () => {

    const colours = useChartColours();

    const { id: groupId } = useParams({ strict: false });
    const {data: group } = useGroup(groupId);

    const [period, setPeriod] = useState<Period>(getPeriod());
    const report = useGroupMonthlyBalancesReport(groupId, period?.startDate, period?.endDate);

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
        <Page title="Group Monthly Balances" breadcrumbs={[{ text: "Groups", route: "/groups" }, { text: group?.name, route: `/groups/${groupId}/manage` }, { text: "Monthly Balances Report" }]}>
            <Section className="mini-filter-panel">
                <MiniPeriodSelector onChange={setPeriod} instant />
            </Section>
            <Section className="report" header="Group Monthly Balances" headerSize={3}>
                <Line id="group-monthly-balances" data={dataset} options={{
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
        </Page >
    );
}

const getStepSize = (values: number[]) => {
    const max = values.reduce((max, d) => d > max ? d : max, -Infinity);
    const magnitude = Math.pow(10, Math.floor(Math.log10(max)));
    const roundedMax = Math.ceil(max / magnitude) * magnitude;
    return roundedMax / 10;
}
