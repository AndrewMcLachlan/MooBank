import { describe, it, expect } from "vitest";
import type { ForecastMonth } from "api/types.gen";
import { projectedActualChartData } from "./forecastChart";

const month = (over: Partial<ForecastMonth>): ForecastMonth => ({
    monthStart: "2024-01-01",
    openingBalance: 0,
    incomeTotal: 0,
    baselineOutgoingsTotal: 0,
    realisedExpensesTotal: 0,
    plannedExpensesTotal: 0,
    closingBalance: 0,
    ...over,
});

describe("projectedActualChartData", () => {
    it("plots projected across every month (dashed) and leaves actual gaps as null (solid)", () => {
        const months = [
            month({ monthStart: "2024-01-01", incomeTotal: 100, actualIncome: 90 }),
            month({ monthStart: "2024-02-01", incomeTotal: 110, actualIncome: null }),
        ];

        const data = projectedActualChartData(
            months,
            (m) => m.incomeTotal,
            (m) => m.actualIncome,
            { projected: "Projected", actual: "Actual" },
            { solid: "#0a0", trend: "#afa" },
        );

        const [projected, actual] = data.datasets;
        expect(data.labels).toEqual(["Jan 24", "Feb 24"]);
        expect(projected.data).toEqual([100, 110]);
        expect(projected.borderDash).toEqual([5, 5]);
        // A missing actual becomes null so the solid line breaks rather than dropping to zero.
        expect(actual.data).toEqual([90, null]);
        expect(actual.spanGaps).toBe(false);
    });

    it("follows income month by month so one-offs are not flattened away", () => {
        // A one-off — a tax refund, say — lands in February on top of the recurring salary, and the
        // projected line has to spike with it rather than sit flat. Income is a single series now
        // that it all comes from planned items, so the month's total carries both.
        const months = [
            month({ monthStart: "2024-01-01", incomeTotal: 100 }),
            month({ monthStart: "2024-02-01", incomeTotal: 600 }),
        ];

        const data = projectedActualChartData(
            months,
            (m) => m.incomeTotal,
            (m) => m.actualIncome,
            { projected: "Projected", actual: "Actual" },
            { solid: "#0a0", trend: "#afa" },
        );

        expect(data.datasets[0].data).toEqual([100, 600]);
    });
});
