import { format, parseISO } from "date-fns";
import type { ChartData, ChartOptions } from "chart.js";
import type { ForecastMonth } from "api/types.gen";
import { formatCurrency } from "utils/currency";

/** The subset of `useChartColours()` the forecast chart needs. */
export interface ForecastChartColours {
    income: string;
    incomeTrend: string;
    grid: string;
}

// The projected series has no semantic colour (it is neither income nor expense), so it keeps a
// fixed blue in both themes; the actual series uses the theme-aware income colour.
const projectedColour = "rgb(53, 162, 235)";
const projectedFill = "rgba(53, 162, 235, 0.5)";

export const forecastChartData = (months: ForecastMonth[], colours: ForecastChartColours): ChartData<"line"> => ({
    labels: months.map((m) => format(parseISO(m.monthStart), "MMM yy")),
    datasets: [
        {
            label: "Projected Balance",
            data: months.map((m) => m.openingBalance),
            borderColor: projectedColour,
            backgroundColor: projectedFill,
            tension: 0.1,
        },
        {
            label: "Actual Balance",
            data: months.map((m) => m.actualBalance ?? null),
            borderColor: colours.income,
            backgroundColor: colours.incomeTrend,
            tension: 0.1,
            spanGaps: false,
        },
    ],
});

/**
 * Builds a projected-vs-actual line pair for the smaller income/expenses charts.
 * Projected spans the whole plan (dashed, trend shade); actual covers only the
 * historical months for which real data exists (solid, full shade, gaps left open).
 */
export const projectedActualChartData = (
    months: ForecastMonth[],
    projected: (month: ForecastMonth) => number,
    actual: (month: ForecastMonth) => number | null | undefined,
    labels: { projected: string; actual: string },
    colours: { solid: string; trend: string },
): ChartData<"line"> => ({
    labels: months.map((m) => format(parseISO(m.monthStart), "MMM yy")),
    datasets: [
        {
            label: labels.projected,
            data: months.map(projected),
            borderColor: colours.trend,
            backgroundColor: colours.trend,
            borderDash: [5, 5],
            tension: 0.1,
            pointRadius: 0,
        },
        {
            label: labels.actual,
            data: months.map((m) => actual(m) ?? null),
            borderColor: colours.solid,
            backgroundColor: colours.solid,
            tension: 0.1,
            spanGaps: false,
            pointRadius: 0,
        },
    ],
});

export const forecastChartOptions = (currencyCode: string, colours: ForecastChartColours): ChartOptions<"line"> => ({
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
