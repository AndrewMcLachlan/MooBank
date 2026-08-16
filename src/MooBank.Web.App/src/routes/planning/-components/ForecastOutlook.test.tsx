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
    // Mirrors moo-ds's Kpi closely enough for the class-based assertions below: the card carries
    // the tone, and the three slots keep their class names.
    Kpi: Object.assign(
        ({ label, tone, className, children }: { label?: React.ReactNode; tone?: string; className?: string; children?: React.ReactNode }) => (
            <section className={`section kpi ${tone ? `kpi-${tone}` : ""} ${className ?? ""}`}>
                <div className="kpi-label">{label}</div>
                {children}
            </section>
        ),
        {
            Value: ({ className, children }: { className?: string; children?: React.ReactNode }) => (
                <div className={`kpi-value ${className ?? ""}`}>{children}</div>
            ),
            Sub: ({ className, children }: { className?: string; children?: React.ReactNode }) => (
                <div className={`kpi-sub ${className ?? ""}`}>{children}</div>
            ),
        },
    ),
    SpinnerContainer: () => <div data-testid="spinner" />,
    Skeleton: Object.assign(
        () => <div data-testid="skeleton" />,
        {
            Text: () => <div data-testid="skeleton" />,
            Circle: () => <div data-testid="skeleton" />,
            Rect: () => <div data-testid="skeleton" />,
            // Distinct testid: the chart placeholder is asserted separately from
            // the KPI band's, which are counted in aggregate.
            Chart: () => <div data-testid="skeleton-chart" />,
        },
    ),
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

const spending = (baselineOutgoingsTotal: number): ForecastMonth => ({ ...month(0), baselineOutgoingsTotal });

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
        expect(container.querySelectorAll(".kpi-value.negative")).toHaveLength(0);
    });

    it("flips to Needs attention and marks Months Below Zero as risk when it runs negative", () => {
        const container = renderOutlook({ monthsBelowZero: 2 });
        expect(screen.getByText("Needs attention")).toBeInTheDocument();
        // Only the Months Below Zero figure is over threshold here.
        const risk = container.querySelectorAll(".kpi-value.negative");
        expect(risk).toHaveLength(1);
        expect(risk[0]).toHaveTextContent("2");
    });

    it("flips to Needs attention and marks Required Monthly Uplift as risk when uplift is needed", () => {
        const container = renderOutlook({ requiredMonthlyUplift: 250 });
        expect(screen.getByText("Needs attention")).toBeInTheDocument();
        expect(container.querySelectorAll(".kpi-value.negative")).toHaveLength(1);
    });

    it("marks Lowest Balance as risk when it drops below zero", () => {
        const container = renderOutlook({ lowestBalance: -500, monthsBelowZero: 1 });
        const risk = container.querySelectorAll(".kpi-value.negative");
        // Lowest Balance (< 0) and Months Below Zero (> 0) are both risks.
        expect(risk).toHaveLength(2);
    });

    it("shows income as the span it covers, not one figure", () => {
        // Income comes from dated planned items, so it moves; an average would read as the answer.
        const { container } = render(
            <ForecastOutlook summary={summary()} months={[month(8000), month(8000), month(5000)]} currencyCode="AUD" />,
        );
        const income = container.querySelector(".kpi-value.income");
        expect(income).toHaveTextContent("5,000");
        expect(income).toHaveTextContent("8,000");
        expect(container.querySelectorAll(".kpi-value.negative")).toHaveLength(0);
    });

    it("shows spending as the span it covers, and explains it with the model", () => {
        // Spending follows income, so it moves too. No single figure describes it: the fixed
        // component is a coefficient rather than an amount, and an average reads as the answer.
        const { container } = render(
            <ForecastOutlook
                summary={summary()}
                months={[spending(10400), spending(12800), spending(14200)]}
                currencyCode="AUD" />,
        );
        const expense = container.querySelector(".kpi-value.expense");
        expect(expense).toHaveTextContent("10,400");
        expect(expense).toHaveTextContent("14,200");
        // The caption states the sensitivity -- what spending does when income moves -- rather than
        // the fixed component, which is where the line crosses zero income and is not an amount
        // anyone ever spends.
        expect(screen.getByText(/moves about .*0\.42 per/)).toBeInTheDocument();
    });

    it("collapses to a single figure when spending does not move", () => {
        const { container } = render(
            <ForecastOutlook summary={summary()} months={[spending(9000), spending(9000)]} currencyCode="AUD" />,
        );
        expect(container.querySelector(".kpi-range-to")).toBeNull();
        expect(container.querySelector(".kpi-value.expense")).toHaveTextContent("9,000");
    });

    it("drops the variable part when there was too little history to relate spending to income", () => {
        // Not a different kind of answer -- the same model with nothing to say about income yet.
        const noFit = summary({ expenses: expenses({ variableComponent: 0, rSquared: 0, dataPoints: 0, fixedComponent: 6457, averageMonthly: 6457 }) });
        render(<ForecastOutlook summary={noFit} months={[spending(6457)]} currencyCode="AUD" />);
        expect(screen.getByText("not enough history to tie this to income")).toBeInTheDocument();
        expect(screen.getByText(/Not enough history/)).toBeInTheDocument();
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

    it("holds the KPI band's shape with placeholders while the summary loads, but claims no verdict", () => {
        const { container } = render(
            <ForecastOutlook months={[]} currencyCode="AUD" loading />,
        );
        // No health pill: "On track" is an assertion about data we don't have yet.
        expect(screen.queryByTestId("badge")).toBeNull();
        // The band is still present, so the chart below it doesn't start high and jump down.
        expect(container.querySelector(".forecast-metrics")).not.toBeNull();
        expect(screen.getAllByTestId("skeleton").length).toBeGreaterThan(0);
        // The chart's slot is held by its skeleton, not by the chart itself: an
        // empty <Line> under a placeholder is furniture drawn from data we do
        // not have yet.
        expect(screen.getByTestId("skeleton-chart")).toBeInTheDocument();
        expect(screen.queryByTestId("forecast-chart")).toBeNull();
    });
});
