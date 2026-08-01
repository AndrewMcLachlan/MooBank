import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import type { ExpenseModel, ForecastMonth, ForecastPlan, ForecastSummary } from "api/types.gen";
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
    Badge: ({ children }: { children?: React.ReactNode }) => <span data-testid="badge">{children}</span>,
    SpinnerContainer: () => <div data-testid="spinner" />,
    // Render the overlay inline so the popover content is assertable without a hover.
    OverlayTrigger: ({ children, overlay }: { children?: React.ReactNode; overlay?: React.ReactNode }) => <>{children}{overlay}</>,
    Popover: Object.assign(
        ({ children }: { children?: React.ReactNode }) => <div>{children}</div>,
        { Body: ({ children }: { children?: React.ReactNode }) => <div>{children}</div> },
    ),
}));

const expenses = (over: Partial<ExpenseModel> = {}): ExpenseModel => ({
    fixedComponent: 1200,
    variableComponent: 0.42,
    rSquared: 0.83,
    dataPoints: 24,
    usingFlatAverage: false,
    flatAverage: 6457,
    modelledIncomeShortfall: 0,
    averageMonthly: 6457,
    ...over,
});

const summary = (over: Partial<ForecastSummary> = {}): ForecastSummary => ({
    lowestBalance: 101536,
    lowestBalanceMonth: "2026-07-01",
    requiredMonthlyUplift: 0,
    monthsBelowZero: 0,
    totalIncome: 303138,
    totalOutgoings: 311925,
    expenses: expenses(),
    ...over,
});

const month = (incomeTotal: number): ForecastMonth => ({
    monthStart: "2026-01-01",
    openingBalance: 0,
    incomeTotal,
    baselineOutgoingsTotal: 0,
    plannedExpensesTotal: 0,
    realisedExpensesTotal: 0,
    closingBalance: 0,
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
        expect(container.querySelectorAll(".metric-value.negative")).toHaveLength(0);
    });

    it("flips to Needs attention and marks Months Below Zero as risk when it runs negative", () => {
        const container = renderOutlook({ monthsBelowZero: 2 });
        expect(screen.getByText("Needs attention")).toBeInTheDocument();
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

    it("averages income across the plan's months rather than quoting a single figure", () => {
        // Income comes from planned items now, so it varies month to month and there is no one
        // number on the plan to read it from.
        const { container } = render(
            <ForecastOutlook summary={summary()} months={[month(8000), month(8000), month(5000)]} currencyCode="AUD" />,
        );
        expect(container.querySelector(".metric-value.income")).toHaveTextContent("7,000");
        expect(container.querySelectorAll(".metric-value.negative")).toHaveLength(0);
    });

    it("leads with both parts of the expense model, not one figure", () => {
        const { container } = render(
            <ForecastOutlook summary={summary()} months={[]} currencyCode="AUD" />,
        );
        const expense = container.querySelector(".metric-value.expense");
        expect(expense).toHaveTextContent("1,200");
        expect(expense).toHaveTextContent("42.0%");
        // The average is still available, but as a caption rather than the answer.
        expect(screen.getByText(/averages/)).toBeInTheDocument();
    });

    it("says so when the fit was rejected, rather than passing the average off as the model", () => {
        const fellBack = summary({ expenses: expenses({ usingFlatAverage: true, rSquared: 0.004, dataPoints: 7 }) });
        render(<ForecastOutlook summary={fellBack} months={[]} currencyCode="AUD" />);
        expect(screen.getByText("flat average · not tied to income")).toBeInTheDocument();
        expect(screen.getByText(/will not move this figure/)).toBeInTheDocument();
    });

    it("warns when the plan models less income than the accounts actually receive", () => {
        // Spending is priced off the larger figure, so the outlook is gloomier than it should be.
        const short = summary({ expenses: expenses({ modelledIncomeShortfall: 7037 }) });
        render(<ForecastOutlook summary={short} months={[]} currencyCode="AUD" />);
        expect(screen.getByText(/7,037/)).toBeInTheDocument();
        expect(screen.getByText(/gloomier than it should be/)).toBeInTheDocument();
    });

    it("stays quiet about the shortfall when the income model is complete", () => {
        render(<ForecastOutlook summary={summary()} months={[]} currencyCode="AUD" />);
        expect(screen.queryByText(/gloomier than it should be/)).toBeNull();
    });

    it("renders neither the health pill nor the KPI band while the summary is still loading", () => {
        const { container } = render(
            <ForecastOutlook months={[]} currencyCode="AUD" loading />,
        );
        expect(screen.queryByTestId("badge")).toBeNull();
        expect(container.querySelector(".forecast-metrics")).toBeNull();
        expect(screen.getByTestId("forecast-chart")).toBeInTheDocument();
    });
});
