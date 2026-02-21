import { format } from "date-fns/format";
import { getMonth } from "date-fns/getMonth";
import { getYear } from "date-fns/getYear";

import { Widget } from "@andrewmclachlan/moo-ds";
import { ChartData } from "chart.js";
import { useChartColours } from "helpers";
import { lastMonth, lastMonthName } from "helpers/dateFns";
import { Bar } from "react-chartjs-2";
import { useBudgetReportForMonth } from "services/BudgetService";

export const BudgetWidget: React.FC = () => {

    const colours = useChartColours();

    const period = lastMonth;

    const { data: report, isLoading } = useBudgetReportForMonth(lastMonth.startDate.getFullYear(), lastMonth.startDate.getMonth());

    const dataset: ChartData<"bar", number[], string> = {
        labels: [formatPeriod(period?.startDate, period?.endDate)],

        datasets: [{
            label: "Budget",
            data: [report?.budgetedAmount ?? 0],
            backgroundColor: colours.income,
        }, {
            label: "Actual",
            data: [Math.abs(report?.actual ?? 0)],
            backgroundColor: colours.expenses,
        }]
    };

    // Calculate the difference to the nearest $10
    const difference = Math.round((((report?.budgetedAmount ?? 0) - Math.abs(report?.actual ?? 0)) / 10.0)) * 10;

    return (
        <Widget header={`Budget - ${lastMonthName}`} size="single" className="report budget" loading={isLoading}>
            {report &&
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
            }
        </Widget>
    )
};

const formatPeriod = (start?: Date, end?: Date): string => {

    if (!start && !end) return "All time";

    const sameYear = getYear(start) === getYear(end);
    const sameMonth = getMonth(start) === getMonth(end);

    if (sameYear && sameMonth) return format(start, "MMM");

    if (sameYear) return `${format(start, "MMM")} - ${format(end, "MMM")}`;

    return `${format(start, "MMM yy")} - ${format(end, "MMM yy")}`;
}
