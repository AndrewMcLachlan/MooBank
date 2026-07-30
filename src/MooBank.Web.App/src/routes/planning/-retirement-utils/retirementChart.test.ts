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
    members: [],
    ...over,
});

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
