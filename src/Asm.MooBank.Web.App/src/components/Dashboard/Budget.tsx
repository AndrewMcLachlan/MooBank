import format from "date-fns/format";
import getMonth from "date-fns/getMonth";
import getYear from "date-fns/getYear";

import { lastMonth } from "helpers/dateFns";
import { Spinner } from "react-bootstrap";
import { Widget } from "./Widget";
import { useChartColours } from "helpers";
import { useBudgetReportForMonth } from "services/BudgetService";
import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";

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

    return (
        <Widget title={`Budget - Last Month`} size={2} className="report" loading={isLoading}>
            <Bar id="inout" data={dataset} options={{
                indexAxis: "y",
                maintainAspectRatio: false,
                scales: {
                    y: {
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