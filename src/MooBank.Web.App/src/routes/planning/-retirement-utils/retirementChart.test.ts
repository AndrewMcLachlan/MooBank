import { describe, it, expect } from "vitest";
import type { RetirementProjectionYear } from "api/types.gen";
import { retirementChartData } from "./retirementChart";
import { fromPercent, toPercent } from "./retirementDefaults";

const colours = { income: "#0a0", incomeTrend: "#afa", grid: "#eee" };

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
    members: [],
    ...over,
});

describe("retirementChartData", () => {
    it("plots the nominal balance solid and today's dollars dashed", () => {
        const years = [
            year({ year: 2026, closingBalance: 100_000, closingBalanceInTodaysDollars: 100_000 }),
            year({ year: 2027, closingBalance: 120_000, closingBalanceInTodaysDollars: 117_000 }),
        ];

        const data = retirementChartData(years, colours);

        expect(data.datasets).toHaveLength(2);
        expect(data.datasets[0].data).toEqual([100_000, 120_000]);
        expect(data.datasets[1].data).toEqual([100_000, 117_000]);
        expect(data.datasets[1].borderDash).toEqual([5, 5]);
    });

    it("produces no data points for an empty projection", () => {
        const data = retirementChartData([], colours);

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
    const years = () => [
        year({ year: 2026, closingBalance: 2_000_000 }),
        year({ year: 2027, closingBalance: 1_500_000 }),
        year({ year: 2028, closingBalance: 900_000 }),
    ];

    const series = (startsBelow?: number) =>
        retirementChartData(years(), colours, startsBelow).datasets.find(d => /pension/i.test(String(d.label)));

    it("is not drawn when there is no level to draw", () => {
        expect(series(0)).toBeUndefined();
        expect(series()).toBeUndefined();
    });

    /**
     * Straight on purpose. The level is one number in today's dollars, and a flat line is what makes
     * the crossing easy to see: above it the pension pays nothing, below it the pension starts.
     */
    it("is a straight line at the level, across every year", () => {
        expect(series(1_048_000)!.data).toEqual([1_048_000, 1_048_000, 1_048_000]);
    });

    it("is dashed and carries no points of its own", () => {
        expect(series(1_048_000)!.borderDash).toEqual([6, 4]);
        expect(series(1_048_000)!.pointRadius).toBe(0);
    });

    it("leaves the balance series untouched", () => {
        const data = retirementChartData(years(), colours, 1_048_000);

        expect(data.datasets).toHaveLength(3);
        expect(data.datasets[0].label).toBe("Projected Balance");
        expect(data.datasets[1].label).toBe("In Today's Dollars");
    });
});
