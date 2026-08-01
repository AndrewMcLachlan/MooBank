import type { ChartData, ChartOptions } from "chart.js";
import type { RetirementProjectionYear } from "api/types.gen";
import { formatCurrency } from "utils/currency";

/** The subset of `useChartColours()` the retirement chart needs. */
export interface RetirementChartColours {
    grid: string;
}

// The balance is neither income nor expense, so it keeps a fixed blue in both themes.
const balanceColour = "rgb(53, 162, 235)";
const balanceFill = "rgba(53, 162, 235, 0.5)";

/** Muted, because the threshold is a reference the balance is read against, not a result. */
const cutOffColour = "rgb(214, 152, 60)";

/**
 * Where the Age Pension cuts out, plotted only from the year someone is old enough for it to mean
 * anything.
 *
 * The balance crossing this line is the year the pension starts, which is otherwise invisible on a
 * balance chart. In today's dollars it is level — the thresholds are indexed, so in real terms they
 * do not move — and a level reference is what makes the crossing readable at a glance.
 */
const cutOffSeries = (years: RetirementProjectionYear[]) =>
    years.map((y) => (y.pensionAssetsCutOffInTodaysDollars > 0 ? y.pensionAssetsCutOffInTodaysDollars : null));

/** Whether anyone in the projection ever reaches an age at which the threshold applies. */
const hasCutOff = (years: RetirementProjectionYear[]) => years.some((y) => y.pensionAssetsCutOff > 0);

/**
 * The balance, in today's dollars.
 *
 * One curve rather than two. A nominal balance climbing to seven figures alongside its real value
 * invited every figure on the page to be read against the wrong one, and a threshold cannot be drawn
 * against both scales at once — which is the whole point of drawing it.
 */
export const retirementChartData = (years: RetirementProjectionYear[]): ChartData<"line"> => ({
    labels: years.map((y) => y.year.toString()),
    datasets: [
        {
            label: "Balance in today's dollars",
            data: years.map((y) => y.closingBalanceInTodaysDollars),
            borderColor: balanceColour,
            backgroundColor: balanceFill,
            tension: 0.1,
        },
        ...(hasCutOff(years) ? [{
            label: "Age pension cuts out above",
            data: cutOffSeries(years),
            borderColor: cutOffColour,
            backgroundColor: cutOffColour,
            borderDash: [2, 3],
            borderWidth: 1.5,
            pointRadius: 0,
            // Gaps before pension age are meant to be gaps, not a line drawn through them.
            spanGaps: false,
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
