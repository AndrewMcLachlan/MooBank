import { describe, it, expect } from "vitest";
import type { CostDataPoint, UsageDataPoint } from "api/types.gen";
import { costPerUnitChartData, rollingAverage, usageChartData } from "./billCharts";

const cost = (over: Partial<CostDataPoint>): CostDataPoint => ({
    date: "2026-01-31",
    accountName: "Electricity",
    usageType: "Consumption",
    averagePricePerUnit: 0.30,
    totalUsage: 400,
    ...over,
});

const usage = (over: Partial<UsageDataPoint>): UsageDataPoint => ({
    date: "2026-01-31",
    accountName: "Electricity",
    usageType: "Consumption",
    usagePerDay: 20,
    ...over,
});

describe("costPerUnitChartData", () => {
    it("draws consumption and feed-in as separate series", () => {
        const result = costPerUnitChartData([
            cost({ averagePricePerUnit: 0.30 }),
            cost({ usageType: "Export", averagePricePerUnit: 0.08 }),
        ]);

        expect(result.series.map(s => s.label)).toEqual(["Electricity — usage", "Electricity — feed-in"]);
        expect(result.series[0].data).toEqual([0.30]);
        expect(result.series[1].data).toEqual([0.08]);
    });

    it("leaves the usage type off an account with no export, so a water account reads as before", () => {
        const result = costPerUnitChartData([
            cost({ accountName: "Water" }),
            cost({ accountName: "Water", date: "2026-02-28" }),
        ]);

        expect(result.series).toHaveLength(1);
        expect(result.series[0].label).toBe("Water");
    });

    it("names the type only for the account that has export, not for the others on the chart", () => {
        const result = costPerUnitChartData([
            cost({ accountName: "Electricity" }),
            cost({ accountName: "Electricity", usageType: "Export", averagePricePerUnit: 0.08 }),
            cost({ accountName: "Water" }),
        ]);

        expect(result.series.map(s => s.label)).toContain("Water");
        expect(result.series.map(s => s.label)).toContain("Electricity — feed-in");
    });

    it("holds a gap where a series has no reading for a date rather than shifting its points", () => {
        // Export started partway through: its first two dates must be null, not the readings from
        // the dates it does have.
        const result = costPerUnitChartData([
            cost({ date: "2026-01-31" }),
            cost({ date: "2026-02-28" }),
            cost({ date: "2026-03-31" }),
            cost({ date: "2026-03-31", usageType: "Export", averagePricePerUnit: 0.08 }),
        ]);

        const exportSeries = result.series.find(s => s.label.includes("feed-in"));
        expect(result.dates).toEqual(["2026-01-31", "2026-02-28", "2026-03-31"]);
        expect(exportSeries?.data).toEqual([null, null, 0.08]);
    });
});

describe("usageChartData", () => {
    it("separates export from consumption and reports that there is some", () => {
        const result = usageChartData([
            usage({ usagePerDay: 20 }),
            usage({ usageType: "Export", usagePerDay: 10 }),
        ]);

        expect(result.hasExport).toBe(true);
        expect(result.consumption).toEqual([20]);
        expect(result.export).toEqual([10]);
    });

    it("reports no export for an account without solar, so the chart keeps one line", () => {
        const result = usageChartData([usage({}), usage({ date: "2026-02-28" })]);

        expect(result.hasExport).toBe(false);
        expect(result.export).toEqual([null, null]);
    });

    it("aligns both series on the same dates", () => {
        const result = usageChartData([
            usage({ date: "2026-01-31", usagePerDay: 20 }),
            usage({ date: "2026-02-28", usagePerDay: 25 }),
            usage({ date: "2026-02-28", usageType: "Export", usagePerDay: 12 }),
        ]);

        expect(result.dates).toEqual(["2026-01-31", "2026-02-28"]);
        expect(result.consumption).toEqual([20, 25]);
        expect(result.export).toEqual([null, 12]);
    });
});

describe("rollingAverage", () => {
    it("averages over the window", () => {
        expect(rollingAverage([10, 20, 30], 3)).toEqual([10, 15, 20]);
    });

    it("skips gaps rather than treating them as zero", () => {
        expect(rollingAverage([10, null, 20], 3)).toEqual([10, 10, 15]);
    });

    it("returns null while there is nothing to average", () => {
        expect(rollingAverage([null, 10], 2)).toEqual([null, 10]);
    });
});
