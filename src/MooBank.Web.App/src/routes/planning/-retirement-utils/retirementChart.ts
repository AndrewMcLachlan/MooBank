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
 * Where the Age Pension cuts out, drawn against the nominal balance it shares a scale with.
 *
 * The balance crossing this line is the year the pension starts topping up the income, which a
 * balance chart otherwise gives no hint of. It climbs because the thresholds are indexed along with
 * the rates, at the same pace as the nominal balance beside it.
 *
 * Only where it applies: before anyone reaches pension age no level of assets pays anything, so
 * there is no threshold to draw rather than one sitting at nought.
 */
const cutOffSeries = (years: RetirementProjectionYear[]) =>
    years.map((y) => (y.pensionAssetsCutOff > 0 ? y.pensionAssetsCutOff : null));

/** Whether anyone in the projection ever reaches an age at which the threshold applies. */
const hasCutOff = (years: RetirementProjectionYear[]) => years.some((y) => y.pensionAssetsCutOff > 0);

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
