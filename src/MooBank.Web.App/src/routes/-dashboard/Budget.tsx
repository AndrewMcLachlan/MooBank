import { format } from "date-fns/format";
import { getMonth } from "date-fns/getMonth";
import { getYear } from "date-fns/getYear";

import { Widget } from "@andrewmclachlan/moo-ds";
import type { ChartData } from "chart.js";
import { useChartColours } from "utils/chartColours";
import { lastMonth, lastMonthName } from "utils/dateFns";
import { Bar } from "react-chartjs-2";
import { useBudgetReportForMonth } from "./-hooks/useBudgetReportForMonth";
import { Amount, WidgetError } from "components";

export const BudgetWidget: React.FC = () => {

    const colours = useChartColours();

    const period = lastMonth;

    const { data: report, isLoading, isError } = useBudgetReportForMonth(lastMonth.startDate.getFullYear(), lastMonth.startDate.getMonth());

    const budgeted = report?.budgetedAmount ?? 0;
    const actual = Math.abs(report?.actual ?? 0);

    const dataset: ChartData<"bar", number[], string> = {
        labels: [formatPeriod(period?.startDate, period?.endDate)],

        datasets: [{
            label: "Budget",
            data: [budgeted],
            backgroundColor: colours.neutralTrend,
        }, {
            label: "Actual",
            data: [actual],
            backgroundColor: actual > budgeted ? colours.expenses : colours.income,
        }]
    };

    // Calculate the difference to the nearest $10
    const difference = Math.round((((report?.budgetedAmount ?? 0) - Math.abs(report?.actual ?? 0)) / 10.0)) * 10;

    return (
        <Widget header={`Budget - ${lastMonthName}`} size="single" className="report budget" loading={isLoading} to={`/budget/report/${lastMonth.startDate.getFullYear()}/${lastMonth.startDate.getMonth()}`}>
            {isError && <WidgetError />}
            {!isError && report &&
                <>
                    {difference >= 0 ?
                        <h4><Amount amount={difference} prefix="$" suffix=" saved" decimalPlaces={0} positiveColour negativeColour /></h4> :
                        <h4><Amount amount={difference} prefix="$" suffix=" overspent" decimalPlaces={0} positiveColour negativeColour /></h4>
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
