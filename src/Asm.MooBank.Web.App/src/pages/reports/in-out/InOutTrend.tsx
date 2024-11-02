import { useInOutTrendReport } from "services";

import { Line } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";

import { Period } from "helpers/dateFns";
import { useChartColours } from "helpers/chartColours";
import { SpinnerContainer } from "@andrewmclachlan/mooapp";

ChartJS.register(...registerables);

export const InOutTrend: React.FC<InOutTrendProps> = ({ accountId, period }) => {

    const colours = useChartColours();

    const report = useInOutTrendReport(accountId!, period?.startDate, period?.endDate);

    const dataset: ChartData<"line", number[], string> = {
        labels: report.data?.income.map(i => i.month) ?? [],

        datasets: [{
            label: "Income",
            data: report.data?.income.map(i => i.amount) ?? [],
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
            data: report.data?.expenses.map(i => Math.abs(i.amount)) ?? [],
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

    return (
        <>
            {report.isLoading && <SpinnerContainer />}
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
        </>
    );
}

export interface InOutTrendProps {
    period: Period;
    accountId: string;
}
