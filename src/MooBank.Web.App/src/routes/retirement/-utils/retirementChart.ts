import type { ChartData, ChartOptions } from "chart.js";
import type { RetirementProjectionYear } from "api/types.gen";
import { formatCurrency } from "utils/currency";

/** The subset of `useChartColours()` the retirement chart needs. */
export interface RetirementChartColours {
    income: string;
    incomeTrend: string;
    grid: string;
}

// The nominal series is neither income nor expense, so it keeps a fixed blue in both themes; the
// today's-dollars series uses the theme-aware income colour.
const nominalColour = "rgb(53, 162, 235)";
const nominalFill = "rgba(53, 162, 235, 0.5)";

export const retirementChartData = (years: RetirementProjectionYear[], colours: RetirementChartColours): ChartData<"line"> => ({
    labels: years.map((y) => y.year.toString()),
    datasets: [
        {
            label: "Projected Balance",
            data: years.map((y) => y.closingBalance),
            borderColor: nominalColour,
            backgroundColor: nominalFill,
            tension: 0.1,
        },
        {
            label: "In Today's Dollars",
            data: years.map((y) => y.closingBalanceInTodaysDollars),
            borderColor: colours.income,
            backgroundColor: colours.incomeTrend,
            borderDash: [5, 5],
            tension: 0.1,
        },
    ],
});

export const retirementChartOptions = (currencyCode: string, colours: RetirementChartColours): ChartOptions<"line"> => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: { position: "top" },
        tooltip: {
            callbacks: {
                label: (context) => `${context.dataset.label}: ${formatCurrency(context.parsed.y, currencyCode)}`,
            },
        },
    },
    scales: {
        y: {
            grid: { color: colours.grid },
            ticks: {
                callback: (value) => formatCurrency(Number(value), currencyCode, 0),
            },
        },
        x: {
            grid: { color: colours.grid },
        },
    },
});
