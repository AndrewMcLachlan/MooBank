import { format } from "date-fns/format";
import type { ChartData, ChartOptions, GridLineOptions, ScriptableScaleContext } from "chart.js";
import type { BudgetMonth } from "api/types.gen";
import { formatCurrency } from "utils/currency";

/** The subset of `useChartColours()` the budget year charts need. */
export interface BudgetYearChartColours {
    income: string;
    expenses: string;
    neutralTrend: string;
    grid: string;
}

// A running surplus is neither income nor expense — and it can be either sign — so it takes a
// fixed blue in both themes rather than a semantic colour, matching the forecast chart.
const cumulativeColour = "rgb(53, 162, 235)";

/**
 * `BudgetMonth.month` is zero-based here (the `/budget/{year}` endpoint's convention, unlike the
 * report endpoints, which are one-based), so it maps straight onto a JS month index.
 */
const monthLabels = (months: BudgetMonth[]) => months.map(m => format(new Date(2000, m.month, 1), "MMM"));

/** Emphasises the zero line, which is what both charts are read against. */
const zeroAwareGrid = (colours: BudgetYearChartColours): Partial<GridLineOptions> => ({
    color: (ctx: ScriptableScaleContext) => ctx.tick.value === 0 ? colours.neutralTrend : colours.grid,
    lineWidth: (ctx: ScriptableScaleContext) => ctx.tick.value === 0 ? 2 : 1,
});

const currencyTicks = {
    callback: (value: string | number) => formatCurrency(Number(value), undefined, 0),
};

/**
 * What each month leaves over. Plotting the remainder rather than income and expenses separately
 * is the point: budgeted income barely varies, so the interesting quantity is the gap, and a
 * month that doesn't pay for itself falls below the axis instead of having to be inferred.
 *
 * Sign is carried by position about the zero line as well as by colour, so the chart still reads
 * without colour vision.
 */
export const budgetSurplusChartData = (months: BudgetMonth[], colours: BudgetYearChartColours): ChartData<"bar", number[], string> => ({
    labels: monthLabels(months),
    datasets: [
        {
            label: "Surplus",
            data: months.map(m => m.remainder),
            backgroundColor: months.map(m => m.remainder < 0 ? colours.expenses : colours.income),
        },
    ],
});

export const budgetSurplusChartOptions = (colours: BudgetYearChartColours): ChartOptions<"bar"> => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        // One series — the section header names it, so a legend would only repeat itself.
        legend: { display: false },
        tooltip: {
            callbacks: {
                label: (context) => formatCurrency(context.parsed.y),
            },
        },
    },
    scales: {
        x: { grid: { color: colours.grid } },
        y: { grid: zeroAwareGrid(colours), ticks: currencyTicks },
    },
});

/**
 * The running total of those surpluses — where the budget actually leaves you, and when it climbs
 * back out of any early-year hole. The final point equals the annual surplus shown in the summary.
 */
export const budgetCumulativeChartData = (months: BudgetMonth[]): ChartData<"line", number[], string> => {
    let running = 0;

    return {
        labels: monthLabels(months),
        datasets: [
            {
                label: "Cumulative surplus",
                data: months.map(m => running += m.remainder),
                borderColor: cumulativeColour,
                backgroundColor: cumulativeColour,
                tension: 0.1,
                pointRadius: 3,
            },
        ],
    };
};

export const budgetCumulativeChartOptions = (colours: BudgetYearChartColours): ChartOptions<"line"> => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { display: false },
        tooltip: {
            callbacks: {
                label: (context) => formatCurrency(context.parsed.y),
            },
        },
    },
    scales: {
        x: { grid: { color: colours.grid } },
        y: { grid: zeroAwareGrid(colours), ticks: currencyTicks },
    },
});
