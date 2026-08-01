import { Section } from "@andrewmclachlan/moo-ds";
import { Line } from "react-chartjs-2";
import type { ForecastMonth } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { forecastChartOptions, projectedActualChartData } from "../-utils/forecastChart";

interface ForecastIncomeExpenseChartsProps {
    months: ForecastMonth[];
    currencyCode: string;
}

export const ForecastIncomeExpenseCharts: React.FC<ForecastIncomeExpenseChartsProps> = ({ months, currencyCode }) => {

    const colours = useChartColours();

    if (months.length === 0) {
        return null;
    }

    const options = forecastChartOptions(currencyCode, colours);

    // Projected totals include planned items, so one-off and scheduled items show as the spikes
    // they are — the recurring/baseline figures alone are flat lines.
    const incomeData = projectedActualChartData(
        months,
        (m) => m.incomeTotal,
        (m) => m.actualIncome,
        { projected: "Projected", actual: "Actual" },
        { solid: colours.income, trend: colours.incomeTrend },
    );

    const expensesData = projectedActualChartData(
        months,
        (m) => m.baselineOutgoingsTotal + m.plannedExpensesTotal,
        (m) => m.actualOutgoings,
        { projected: "Projected", actual: "Actual" },
        { solid: colours.expenses, trend: colours.expensesTrend },
    );

    return (
        <div className="forecast-io-charts">
            <Section header="Income">
                <div className="forecast-mini-chart"><Line data={incomeData} options={options} /></div>
            </Section>
            <Section header="Expenses">
                <div className="forecast-mini-chart"><Line data={expensesData} options={options} /></div>
            </Section>
        </div>
    );
};
