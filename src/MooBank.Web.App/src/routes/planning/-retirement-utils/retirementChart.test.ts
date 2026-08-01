import { describe, it, expect } from "vitest";
import type { RetirementProjectionYear } from "api/types.gen";
import { retirementChartData } from "./retirementChart";
import { fromPercent, toPercent } from "./retirementDefaults";

const year = (over: Partial<RetirementProjectionYear>): RetirementProjectionYear => ({
    year: 2026,
    openingBalance: 0,
    contributions: 0,
    investmentReturn: 0,
    closingBalance: 0,
    costs: 0,
    closingBalanceInTodaysDollars: 0,
    allRetired: false,
    drawdown: 0,
    drawdownInTodaysDollars: 0,
    pension: 0,
    totalIncome: 0,
    totalIncomeInTodaysDollars: 0,
    pensionInTodaysDollars: 0,
    pensionAssetsCutOff: 0,
    pensionAssetsCutOffInTodaysDollars: 0,
    members: [],
    ...over,
});

describe("retirementChartData", () => {
    /**
     * One curve, in today's dollars. A nominal balance alongside its real value invited every figure
     * on the page to be read against the wrong one, and the pension threshold cannot sit against both
     * scales at once.
     */
    it("plots the balance in today's dollars", () => {
        const years = [
            year({ year: 2026, closingBalance: 100_000, closingBalanceInTodaysDollars: 100_000 }),
            year({ year: 2027, closingBalance: 120_000, closingBalanceInTodaysDollars: 117_000 }),
        ];

        const data = retirementChartData(years);

        expect(data.datasets).toHaveLength(1);
        expect(data.datasets[0].data).toEqual([100_000, 117_000]);
        expect(data.datasets[0].data).not.toContain(120_000);
    });

    it("produces no data points for an empty projection", () => {
        const data = retirementChartData([]);

        expect(data.labels).toEqual([]);
        expect(data.datasets.every(d => d.data.length === 0)).toBe(true);
    });
});

describe("rate conversion", () => {
    it.each([
        [0.065, 6.5],
        [0.12, 12],
        [0.025, 2.5],
        [0, 0],
    ])("shows the fraction %s as %s%%", (rate, percent) => {
        expect(toPercent(rate)).toBe(percent);
    });

    it("treats a missing rate as zero rather than NaN", () => {
        expect(toPercent(undefined as unknown as number)).toBe(0);
        expect(fromPercent(undefined as unknown as number)).toBe(0);
    });

    it("round-trips a rate through the form and back", () => {
        expect(fromPercent(toPercent(0.065))).toBeCloseTo(0.065, 10);
        expect(fromPercent(toPercent(0.12))).toBeCloseTo(0.12, 10);
    });
});

/**
 * The Age Pension threshold drawn against the balance.
 *
 * The year the balance crosses it is the year the pension starts, which a balance chart otherwise
 * gives no hint of.
 */
describe("the pension threshold", () => {
    // The threshold is level in today's dollars, however much it climbs in nominal terms.
    const withCutOff = () => [
        year({ year: 2026, closingBalanceInTodaysDollars: 2_000_000 }),
        year({ year: 2027, closingBalanceInTodaysDollars: 1_500_000, pensionAssetsCutOff: 1_100_000, pensionAssetsCutOffInTodaysDollars: 1_048_000 }),
        year({ year: 2028, closingBalanceInTodaysDollars: 900_000, pensionAssetsCutOff: 1_130_000, pensionAssetsCutOffInTodaysDollars: 1_048_000 }),
    ];

    const cutOffSeries = (years: Parameters<typeof retirementChartData>[0]) =>
        retirementChartData(years).datasets.find(d => /pension/i.test(String(d.label)));

    it("is not drawn when nobody ever reaches pension age", () => {
        const data = retirementChartData([year({ year: 2026 }), year({ year: 2027 })]);

        expect(data.datasets).toHaveLength(1);
        expect(data.datasets.some(d => /pension/i.test(String(d.label)))).toBe(false);
    });

    it("is drawn dotted, without points, once it applies", () => {
        const series = cutOffSeries(withCutOff());

        expect(series).toBeDefined();
        expect(series!.borderDash).toEqual([2, 3]);
        expect(series!.pointRadius).toBe(0);
    });

    /** Years before pension age are gaps, not a line dropped to nought. */
    it("leaves a gap for years the threshold does not apply to", () => {
        const series = cutOffSeries(withCutOff());

        expect(series!.data).toEqual([null, 1_048_000, 1_048_000]);
        expect(series!.spanGaps).toBe(false);
    });

    /**
     * Level in today's dollars, which is what makes the crossing readable: the balance falls, the
     * threshold does not, and where they meet is the year the pension starts.
     */
    it("is level, in the same money as the balance", () => {
        const series = cutOffSeries(withCutOff());

        expect(series!.data[1]).toBe(series!.data[2]);
        // And the nominal figures, which do climb, are not what was plotted.
        expect(series!.data).not.toContain(1_130_000);
    });
});
