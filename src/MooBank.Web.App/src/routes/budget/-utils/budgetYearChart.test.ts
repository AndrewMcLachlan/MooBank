import { describe, it, expect } from "vitest";
import type { BudgetMonth } from "api/types.gen";
import { budgetCumulativeChartData, budgetSurplusChartData } from "./budgetYearChart";

const colours = { income: "#0a0", expenses: "#f00", neutralTrend: "#999", grid: "#eee" };

const month = (over: Partial<BudgetMonth>): BudgetMonth => ({
    month: 0,
    income: 0,
    expenses: 0,
    remainder: 0,
    ...over,
});

describe("budgetSurplusChartData", () => {
    it("plots the remainder, so a month that doesn't pay for itself goes below the axis", () => {
        const months = [
            month({ month: 0, income: 11522, expenses: 19239.6, remainder: -7717.6 }),
            month({ month: 1, income: 11522, expenses: 7137.67, remainder: 4384.33 }),
        ];

        expect(budgetSurplusChartData(months, colours).datasets[0].data).toEqual([-7717.6, 4384.33]);
    });

    it("colours shortfall months as expense and the rest as income", () => {
        const months = [
            month({ month: 0, remainder: -100 }),
            month({ month: 1, remainder: 100 }),
            month({ month: 2, remainder: 0 }), // breaking exactly even is not a shortfall
        ];

        expect(budgetSurplusChartData(months, colours).datasets[0].backgroundColor)
            .toEqual([colours.expenses, colours.income, colours.income]);
    });

    it("labels months from the zero-based month index the budget endpoint returns", () => {
        // Guards the 0-based/1-based split between /budget/{year} (0-based, here) and the
        // report endpoints (1-based) — reading this as 1-based shifts every label by a month.
        const months = [month({ month: 0 }), month({ month: 6 }), month({ month: 11 })];

        expect(budgetSurplusChartData(months, colours).labels).toEqual(["Jan", "Jul", "Dec"]);
    });
});

describe("budgetCumulativeChartData", () => {
    it("accumulates the remainder month by month", () => {
        const months = [
            month({ month: 0, remainder: -7717.6 }),
            month({ month: 1, remainder: 4384.33 }),
            month({ month: 2, remainder: 6416.5 }),
        ];

        const data = budgetCumulativeChartData(months).datasets[0].data as number[];

        expect(data[0]).toBeCloseTo(-7717.6, 2);
        expect(data[1]).toBeCloseTo(-3333.27, 2);
        // Crosses back above zero in the third month.
        expect(data[2]).toBeCloseTo(3083.23, 2);
    });

    it("ends on the annual surplus, so the chart agrees with the summary tile", () => {
        const months = [
            month({ month: 0, remainder: 1000 }),
            month({ month: 1, remainder: -250 }),
            month({ month: 2, remainder: 500 }),
        ];
        const total = months.reduce((sum, m) => sum + m.remainder, 0);

        const data = budgetCumulativeChartData(months).datasets[0].data as number[];

        expect(data[data.length - 1]).toBe(total);
    });

    it("returns empty series for a budget with no months rather than throwing", () => {
        expect(budgetCumulativeChartData([]).datasets[0].data).toEqual([]);
        expect(budgetSurplusChartData([], colours).datasets[0].data).toEqual([]);
    });
});
