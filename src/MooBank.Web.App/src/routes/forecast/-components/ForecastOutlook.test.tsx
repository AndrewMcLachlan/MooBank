import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import type { ForecastPlan, ForecastSummary } from "api/types.gen";
import { ForecastOutlook } from "./ForecastOutlook";

// The chart pulls in react-chartjs-2 (canvas) and theme context; the Outlook's own logic is the
// health verdict + KPI risk colouring, so stub the chart to keep the test deterministic.
vi.mock("./ForecastChart", () => ({
    ForecastChart: () => <div data-testid="forecast-chart" />,
}));

// Section renders through moo-ds's LinkProvider, which the app supplies but a unit test does not.
// Stub the two primitives this component uses so the test stays provider-free and focused.
vi.mock("@andrewmclachlan/moo-ds", () => ({
    Section: ({ header, children }: { header?: React.ReactNode; children?: React.ReactNode }) => (
        <section>{header}{children}</section>
    ),
    SpinnerContainer: () => <div data-testid="spinner" />,
}));

const summary = (over: Partial<ForecastSummary> = {}): ForecastSummary => ({
    lowestBalance: 101536,
    lowestBalanceMonth: "2026-07-01",
    requiredMonthlyUplift: 0,
    monthsBelowZero: 0,
    totalIncome: 303138,
    totalOutgoings: 311925,
    monthlyBaselineOutgoings: 6457,
    regression: null,
    ...over,
});

const renderOutlook = (over?: Partial<ForecastSummary>) => {
    const { container } = render(
        <ForecastOutlook summary={summary(over)} months={[]} currencyCode="AUD" />,
    );
    return container;
};

describe("ForecastOutlook", () => {
    it("reads On track and colours no KPI as risk when the forecast is healthy", () => {
        const container = renderOutlook();
        expect(screen.getByText("On track")).toBeInTheDocument();
        expect(container.querySelector(".health-pill.on-track")).not.toBeNull();
        expect(container.querySelectorAll(".metric-value.negative")).toHaveLength(0);
    });

    it("flips to Needs attention and marks Months Below Zero as risk when it runs negative", () => {
        const container = renderOutlook({ monthsBelowZero: 2 });
        expect(screen.getByText("Needs attention")).toBeInTheDocument();
        expect(container.querySelector(".health-pill.attention")).not.toBeNull();
        // Only the Months Below Zero figure is over threshold here.
        const risk = container.querySelectorAll(".metric-value.negative");
        expect(risk).toHaveLength(1);
        expect(risk[0]).toHaveTextContent("2");
    });

    it("flips to Needs attention and marks Required Monthly Uplift as risk when uplift is needed", () => {
        const container = renderOutlook({ requiredMonthlyUplift: 250 });
        expect(screen.getByText("Needs attention")).toBeInTheDocument();
        expect(container.querySelectorAll(".metric-value.negative")).toHaveLength(1);
    });

    it("marks Lowest Balance as risk when it drops below zero", () => {
        const container = renderOutlook({ lowestBalance: -500, monthsBelowZero: 1 });
        const risk = container.querySelectorAll(".metric-value.negative");
        // Lowest Balance (< 0) and Months Below Zero (> 0) are both risks.
        expect(risk).toHaveLength(2);
    });

    it("renders the monthly income and expenses cards from the plan and summary", () => {
        const plan = {
            name: "My Forecast",
            startDate: "2024-12-01",
            endDate: "2027-12-01",
            incomeStrategy: { manualRecurring: { amount: 8192.92 } },
            outgoingStrategy: { mode: "IncomeCorrelated" },
        } as ForecastPlan;
        const { container } = render(
            <ForecastOutlook plan={plan} summary={summary({ monthlyBaselineOutgoings: 6457 })} months={[]} currencyCode="AUD" />,
        );
        expect(container.querySelector(".metric-value.income")).toHaveTextContent("8,192.92");
        expect(container.querySelector(".metric-value.expense")).toHaveTextContent("6,457");
        // Income and expenses use their own semantic classes, not the risk .negative marker.
        expect(container.querySelectorAll(".metric-value.negative")).toHaveLength(0);
    });

    it("notes the flat-average fallback on the expenses card when the correlation is weak", () => {
        const plan = { startDate: "2024-12-01", endDate: "2027-12-01", outgoingStrategy: { mode: "IncomeCorrelated" } } as ForecastPlan;
        const fellBack = summary({ regression: { fellBackToFlatAverage: true, rSquared: 0.004 } as ForecastSummary["regression"] });
        render(<ForecastOutlook plan={plan} summary={fellBack} months={[]} currencyCode="AUD" />);
        expect(screen.getByText("income-correlated · flat average")).toBeInTheDocument();
    });

    it("renders neither the health pill nor the KPI band while the summary is still loading", () => {
        const { container } = render(
            <ForecastOutlook months={[]} currencyCode="AUD" loading />,
        );
        expect(container.querySelector(".health-pill")).toBeNull();
        expect(container.querySelector(".forecast-metrics")).toBeNull();
        expect(screen.getByTestId("forecast-chart")).toBeInTheDocument();
    });
});
