import type { CostDataPoint, UsageDataPoint } from "api/types.gen";

/**
 * A bill's metered quantities are read per usage type. Consumption and export appear on the same
 * bill and move independently — a feed-in tariff is not a discount on the rate paid — so they are
 * kept as separate series rather than combined into a single figure for the account.
 */

export interface CostSeries {
    label: string;
    data: (number | null)[];
}

export interface CostChartData {
    dates: string[];
    series: CostSeries[];
}

/**
 * One series per account and usage type. The usage type is only named where the account actually
 * has export, so a plain electricity or water account keeps the label it had before.
 */
export const costPerUnitChartData = (points: CostDataPoint[]): CostChartData => {
    const dates = [...new Set(points.map(d => d.date))].sort();

    const keys = [...new Map(points.map(d => [`${d.accountName}\u0000${d.usageType}`, d])).values()];

    const hasExport = (accountName: string) =>
        points.some(d => d.accountName === accountName && d.usageType === "Export");

    return {
        dates,
        series: keys.map(k => ({
            label: hasExport(k.accountName)
                ? `${k.accountName} — ${k.usageType === "Export" ? "feed-in" : "usage"}`
                : k.accountName,
            // Indexed by date rather than by filter order: a series with no reading for a date must
            // leave a gap at that position, not shift its remaining points left.
            data: dates.map(date =>
                points.find(d => d.date === date && d.accountName === k.accountName && d.usageType === k.usageType)
                    ?.averagePricePerUnit ?? null),
        })),
    };
};

export interface UsageChartSeries {
    dates: string[];
    consumption: (number | null)[];
    export: (number | null)[];
    hasExport: boolean;
}

/**
 * Consumption drives the chart and its rolling average; export is returned alongside so it can be
 * drawn as its own line. Both are indexed by the same dates, so a period with only one of them
 * leaves a gap rather than misaligning the other.
 */
export const usageChartData = (points: UsageDataPoint[]): UsageChartSeries => {
    const dates = [...new Set(points.map(d => d.date))].sort();
    const forType = (usageType: string) =>
        dates.map(date => points.find(d => d.date === date && d.usageType === usageType)?.usagePerDay ?? null);

    return {
        dates,
        consumption: forType("Consumption"),
        export: forType("Export"),
        hasExport: points.some(d => d.usageType === "Export"),
    };
};

/** A rolling mean over the last `windowSize` readings, skipping gaps. */
export const rollingAverage = (data: (number | null)[], windowSize: number): (number | null)[] =>
    data.map((_, index) => {
        const start = Math.max(0, index - windowSize + 1);
        const window = data.slice(start, index + 1).filter((v): v is number => v !== null);
        if (window.length === 0) return null;
        return window.reduce((sum, v) => sum + v, 0) / window.length;
    });
