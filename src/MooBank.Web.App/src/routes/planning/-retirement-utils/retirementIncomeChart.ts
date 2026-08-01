import type { ChartData, ChartOptions } from "chart.js";
import type { RetirementProjectionYear } from "api/types.gen";
import { formatCurrency } from "utils/currency";

/**
 * A colour per member, so each person's contribution to the household's income keeps the same
 * colour down the whole chart. Distinct hues rather than a gradient, because these are separate
 * people and not points on a scale.
 */
const memberColours = [
    "rgb(166, 42, 121)",
    "rgb(232, 132, 187)",
    "rgb(122, 178, 92)",
    "rgb(214, 152, 60)",
];

/**
 * Where the retirement income starts, so the chart shows retirement rather than the decades before
 * it.
 *
 * Keyed on total income, not the drawdown: a household whose pension covers its whole target draws
 * nothing from super, and the chart would otherwise be empty for it.
 */
const drawdownYears = (years: RetirementProjectionYear[]) => {
    const first = years.findIndex((y) => y.totalIncome > 0);

    return first < 0 ? [] : years.slice(first);
};

/** The Age Pension keeps one colour of its own, distinct from the members'. */
const pensionColour = "rgb(53, 132, 196)";

/**
 * Whether there is a retirement income to chart at all. A plan with no target income draws nothing,
 * so there would be nothing to show.
 */
export const hasRetirementIncome = (years: RetirementProjectionYear[]) => drawdownYears(years).length > 0;

/**
 * The household's retirement income, stacked by whose balance funds it.
 *
 * Plotted against the first member's age rather than the calendar year: a retirement plan is read in
 * terms of how old you are. Members with different birthdays reach a given year at different ages,
 * so the axis follows the first of them and the tooltip names the year.
 *
 * Every figure is in today's dollars, because the target income the plan is built around is stated
 * that way. In nominal terms the same plan climbs year on year and reconciles with nothing on the
 * page — a couple's pension at the published maximum reads as a hundred thousand thirty years out,
 * which looks like an error rather than the indexation it is. In today's dollars the total sits flat
 * at the target, which is the plan's actual promise.
 */
export const retirementIncomeChartData = (years: RetirementProjectionYear[]): ChartData<"bar"> => {
    const drawing = drawdownYears(years);

    // Nothing to plot: return an empty chart rather than a lone empty pension series.
    if (drawing.length === 0) return { labels: [], datasets: [] };

    // Members are consistent across years, so the first year fixes the order and the colours.
    const members = drawing[0].members;

    return {
        labels: drawing.map((y) => (y.members[0]?.age ?? y.year).toString()),
        datasets: [
            ...members.map((member, index) => ({
                label: `Income from ${member.name}'s super`,
                data: drawing.map((y) => y.members.find((m) => m.memberId === member.memberId)?.drawdownInTodaysDollars ?? 0),
                backgroundColor: memberColours[index % memberColours.length],
            })),
            // Stacked last so it sits under the super income, which is the order these charts are
            // conventionally read: the pension is the floor the household falls back to.
            {
                label: "Age pension",
                data: drawing.map((y) => y.pensionInTodaysDollars),
                backgroundColor: pensionColour,
            },
        ],
    };
};

export const retirementIncomeChartOptions = (
    currencyCode: string,
    colours: { grid: string },
    years: RetirementProjectionYear[],
): ChartOptions<"bar"> => {
    const drawing = drawdownYears(years);

    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { position: "top" },
            tooltip: {
                callbacks: {
                    // The axis shows an age, so the year it corresponds to belongs in the tooltip.
                    title: (items) => {
                        const year = drawing[items[0]?.dataIndex ?? 0]?.year;

                        return year ? `Age ${items[0].label} (${year})` : `Age ${items[0]?.label}`;
                    },
                    label: (context) => `${context.dataset.label}: ${formatCurrency(context.parsed.y, currencyCode)}`,
                    footer: (items) => {
                        const total = items.reduce((sum, item) => sum + item.parsed.y, 0);

                        return `Total: ${formatCurrency(total, currencyCode)}`;
                    },
                },
            },
        },
        scales: {
            x: {
                stacked: true,
                grid: { color: colours.grid },
                title: { display: true, text: "Age" },
            },
            y: {
                stacked: true,
                grid: { color: colours.grid },
                ticks: {
                    callback: (value) => formatCurrency(Number(value), currencyCode, 0),
                },
            },
        },
    };
};
