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

/** Muted, because the threshold is a reference the balance is read against, not a result. */
const cutOffColour = "rgb(214, 152, 60)";

/**
 * The level below which the Age Pension starts, drawn flat across the whole chart.
 *
 * Straight on purpose. It is one number in today's dollars — the thresholds are indexed, so in real
 * terms the level does not move — and a straight line is what makes the crossing easy to see: above
 * it the pension pays nothing, below it the pension starts topping the income up.
 */
const cutOffSeries = (years: RetirementProjectionYear[], startsBelow: number) => years.map(() => startsBelow);

export const retirementChartData = (
    years: RetirementProjectionYear[],
    colours: RetirementChartColours,
    pensionStartsBelow = 0,
): ChartData<"line"> => ({
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
        ...(pensionStartsBelow > 0 ? [{
            label: "Age pension starts below",
            data: cutOffSeries(years, pensionStartsBelow),
            borderColor: cutOffColour,
            backgroundColor: cutOffColour,
            borderDash: [6, 4],
            borderWidth: 2,
            pointRadius: 0,
            tension: 0,
        }] : []),
    ],
});

export const retirementChartOptions = (currencyCode: string, colours: RetirementChartColours): ChartOptions<"line"> => ({
    responsive: true,
    maintainAspectRatio: false,
    // Both series for the year the pointer is nearest, so the nominal balance and what it is worth
    // today can be read together — which is the only reason to plot them on the same axes.
    interaction: { mode: "index", intersect: false },
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
