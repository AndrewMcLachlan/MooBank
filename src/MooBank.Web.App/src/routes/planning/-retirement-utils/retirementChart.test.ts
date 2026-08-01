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

const colours = { income: "#0a0", incomeTrend: "#afa", grid: "#eee" };

describe("retirementChartData", () => {
    it("plots the nominal balance solid and today's dollars dashed", () => {
        const years = [
            year({ year: 2026, closingBalance: 100_000, closingBalanceInTodaysDollars: 100_000 }),
            year({ year: 2027, closingBalance: 120_000, closingBalanceInTodaysDollars: 117_000 }),
        ];

        const data = retirementChartData(years, { income: "#0a0", incomeTrend: "#afa", grid: "#eee" });

        const [nominal, real] = data.datasets;
        expect(data.labels).toEqual(["2026", "2027"]);
        expect(nominal.data).toEqual([100_000, 120_000]);
        expect(nominal.borderDash).toBeUndefined();
        expect(real.data).toEqual([100_000, 117_000]);
        // The real series is dashed so it reads as a restatement of the same balance.
        expect(real.borderDash).toEqual([5, 5]);
    });

    it("produces no data points for an empty projection", () => {
        const data = retirementChartData([], { income: "#0a0", incomeTrend: "#afa", grid: "#eee" });

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
    const withCutOff = () => [
        year({ year: 2026, closingBalance: 2_000_000 }),
        year({ year: 2027, closingBalance: 1_500_000, pensionAssetsCutOff: 1_100_000 }),
        year({ year: 2028, closingBalance: 900_000, pensionAssetsCutOff: 1_130_000 }),
    ];

    const cutOffSeries = (years: Parameters<typeof retirementChartData>[0]) =>
        retirementChartData(years, colours).datasets.find(d => /pension/i.test(String(d.label)));

    it("is not drawn when nobody ever reaches pension age", () => {
        const data = retirementChartData([year({ year: 2026 }), year({ year: 2027 })], colours);

        expect(data.datasets).toHaveLength(2);
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

        expect(series!.data).toEqual([null, 1_100_000, 1_130_000]);
        expect(series!.spanGaps).toBe(false);
    });

    /** It rises with the indexation applied to the rates, so it shares the nominal balance's scale. */
    it("is plotted in the same money as the nominal balance", () => {
        const series = cutOffSeries(withCutOff());

        expect(series!.data[2]).toBeGreaterThan(series!.data[1] as number);
    });
});
