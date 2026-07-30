import { describe, it, expect } from "vitest";
import type { RetirementMemberYear, RetirementProjectionYear } from "api/types.gen";
import { hasRetirementIncome, retirementIncomeChartData } from "./retirementIncomeChart";

const selfId = "11111111-1111-1111-1111-111111111111";
const spouseId = "22222222-2222-2222-2222-222222222222";

const member = (over: Partial<RetirementMemberYear>): RetirementMemberYear => ({
    memberId: selfId,
    name: "Self",
    age: 60,
    contributions: 0,
    investmentReturn: 0,
    costs: 0,
    drawdown: 0,
    closingBalance: 0,
    ...over,
});

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

/** Two accumulating years, then two drawing years. */
const projection = (): RetirementProjectionYear[] => [
    year({ year: 2026, members: [member({ age: 63 }), member({ memberId: spouseId, name: "Spouse", age: 61 })] }),
    year({ year: 2027, members: [member({ age: 64 }), member({ memberId: spouseId, name: "Spouse", age: 62 })] }),
    year({
        year: 2028,
        drawdown: 40_000,
        totalIncome: 40_000,
        members: [
            member({ age: 65, drawdown: 30_000 }),
            member({ memberId: spouseId, name: "Spouse", age: 63, drawdown: 10_000 }),
        ],
    }),
    year({
        year: 2029,
        drawdown: 40_000,
        totalIncome: 40_000,
        members: [
            member({ age: 66, drawdown: 32_000 }),
            member({ memberId: spouseId, name: "Spouse", age: 64, drawdown: 8_000 }),
        ],
    }),
];

describe("hasRetirementIncome", () => {
    it("is false when nothing is ever drawn", () => {
        expect(hasRetirementIncome([year({}), year({})])).toBe(false);
    });

    it("is true once a year draws an income", () => {
        expect(hasRetirementIncome(projection())).toBe(true);
    });
});

describe("retirementIncomeChartData", () => {
    /** The chart is about retirement, so the decades of accumulation before it are not plotted. */
    it("starts at the first year an income is drawn", () => {
        const data = retirementIncomeChartData(projection());

        expect(data.labels).toEqual(["65", "66"]);
    });

    it("gives each member their own stacked series, with the pension last", () => {
        const data = retirementIncomeChartData(projection());

        expect(data.datasets.map(d => d.label)).toEqual(["Income from Self's super", "Income from Spouse's super", "Age pension"]);
        expect(data.datasets[0].data).toEqual([30_000, 32_000]);
        expect(data.datasets[1].data).toEqual([10_000, 8_000]);
    });

    it("plots the pension as its own series", () => {
        const years = projection();
        years[2] = year({ year: 2028, drawdown: 30_000, pension: 10_000, totalIncome: 40_000, members: [member({ age: 65, drawdown: 30_000 })] });

        const data = retirementIncomeChartData(years);
        const pension = data.datasets.find(d => d.label === "Age pension");

        expect(pension?.data[0]).toBe(10_000);
    });

    /**
     * A household whose pension covers its whole target draws nothing from super. Keyed on the
     * drawdown, the chart would show it nothing at all.
     */
    it("charts a year funded entirely by the pension", () => {
        const years = [
            year({ year: 2026, members: [member({ age: 66 })] }),
            year({ year: 2027, pension: 45_000, totalIncome: 45_000, members: [member({ age: 67 })] }),
        ];

        expect(hasRetirementIncome(years)).toBe(true);
        expect(retirementIncomeChartData(years).labels).toEqual(["67"]);
    });

    it("gives the pension a colour of its own", () => {
        const data = retirementIncomeChartData(projection());
        const memberColours = data.datasets.filter(d => d.label !== "Age pension").map(d => d.backgroundColor);
        const pensionColour = data.datasets.find(d => d.label === "Age pension")?.backgroundColor;

        expect(memberColours).not.toContain(pensionColour);
    });

    it("gives the members different colours", () => {
        const data = retirementIncomeChartData(projection());

        expect(data.datasets[0].backgroundColor).not.toEqual(data.datasets[1].backgroundColor);
    });

    /**
     * A member whose balance is exhausted stops appearing in later years' member lists in principle;
     * matching by id rather than by position keeps the series aligned regardless.
     */
    it("reads each member by id rather than by position", () => {
        const years = projection();
        years[3] = year({
            year: 2029,
            drawdown: 8_000,
            totalIncome: 8_000,
            members: [member({ memberId: spouseId, name: "Spouse", age: 64, drawdown: 8_000 })],
        });

        const data = retirementIncomeChartData(years);

        expect(data.datasets[0].data).toEqual([30_000, 0]);
        expect(data.datasets[1].data).toEqual([10_000, 8_000]);
    });

    it("has nothing to plot when no income is drawn", () => {
        const data = retirementIncomeChartData([year({}), year({})]);

        expect(data.labels).toEqual([]);
        expect(data.datasets).toEqual([]);
    });
});
