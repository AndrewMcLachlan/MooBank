import { useInOutTrendReport } from "../../../-hooks/useInOutTrendReport";

import { Line } from "react-chartjs-2";
import type { ChartData } from "chart.js";

import type { Period } from "models/dateFns";
import { useChartColours } from "utils/chartColours";
import { Skeleton } from "@andrewmclachlan/moo-ds";


export const InOutTrend: React.FC<InOutTrendProps> = ({ accountId, period }) => {

    const colours = useChartColours();

    const report = useInOutTrendReport(accountId!, period?.startDate, period?.endDate);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.income.map(i => i.month) ?? [],

        datasets: [{
            label: "Income",
            data: report.data?.income.map(i => i.grossAmount) ?? [],
            backgroundColor: colours.income,
            borderColor: colours.income,
            // @ts-expect-error Not a known property for some reason
            trendlineLinear: {
                colorMin: colours.incomeTrend,
                colorMax: colours.incomeTrend,
                lineStyle: "solid",
                width: 2,
            }
        }, {
            label: "Expenses",
            data: report.data?.expenses.map(i => Math.abs(i.grossAmount)) ?? [],
            backgroundColor: colours.expenses,
            borderColor: colours.expenses,
            // @ts-expect-error Not a known property for some reason
            trendlineLinear: {
                colorMin: colours.expensesTrend,
                colorMax: colours.expensesTrend,
                lineStyle: "solid",
                width: 2,
            }
        }]
    };

    // The chart's shape is known before its data is, so hold the space with a
    // skeleton rather than a spinner. Returning early also keeps an empty
    // <Line> from being painted underneath the placeholder while it loads.
    if (report.isLoading) return <Skeleton.Chart variant="line" count={2} />;

    return (
            <Line id="inout" data={dataset} options={{
                maintainAspectRatio: false,
                scales: {
                    y: {
                        suggestedMin: 0,
                        ticks: {
                            stepSize: 5000,
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
    );
}

export interface InOutTrendProps {
    period: Period;
    accountId: string;
}
