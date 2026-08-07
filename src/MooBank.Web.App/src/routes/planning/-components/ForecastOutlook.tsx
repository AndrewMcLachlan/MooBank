import { Badge, Kpi, OverlayTrigger, Popover, Section, Skeleton, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { MetricsSkeleton } from "./MetricsSkeleton";
import { format, parseISO } from "date-fns";
import type { ExpenseModel, ForecastMonth, ForecastPlan, ForecastSummary } from "api/types.gen";
import { Amount } from "components";
import { formatCurrency } from "utils/currency";
import { ForecastChart } from "./ForecastChart";

interface ForecastOutlookProps {
    plan?: ForecastPlan;
    summary?: ForecastSummary;
    months: ForecastMonth[];
    currencyCode: string;
    loading?: boolean;
}

const percent = (rate: number) =>
    (rate * 100).toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 });

/**
 * A figure that moves month to month, shown as the span it moves through.
 *
 * A single number cannot describe it, and every single number tried here read as though it were the
 * answer: the fixed component was a coefficient rather than an amount at all, and an average is
 * still one figure standing in for a line that rises and falls. The span says what the chart says.
 */
const MonthlyRange: React.FC<{ months: number[]; currencyCode: string; tone: string }> = ({ months, currencyCode, tone }) => {
    const low = months.length ? Math.min(...months) : 0;
    const high = months.length ? Math.max(...months) : 0;

    return (
        <div className={`kpi-value ${tone}`}>
            <Amount amount={low} currencyCode={currencyCode} />
            {Math.round(high - low) > 0 && (
                <>
                    <span className="kpi-range-to">to</span>
                    <Amount amount={high} currencyCode={currencyCode} />
                </>
            )}
        </div>
    );
};

// The model behind the expenses span, stated as what it is useful for: how much spending moves when
// income does. The fitted line also has a fixed part, but that is where it crosses zero income --
// a construction artefact rather than an amount anyone ever spends, and reading it as fixed costs is
// the mistake that produced a phantom month at the household's whole outgoings. It stays in the
// detail for checking the model, and out of the way of using it.
const ExpenseModelNote: React.FC<{ expenses: ExpenseModel; currencyCode: string }> = ({ expenses, currencyCode }) => (
    <div className="kpi-sub">
        <OverlayTrigger placement="bottom" overlay={
            <Popover id="forecast-expense-popover">
                <Popover.Body>
                    <div className="expense-model-detail">
                        <div>Each month is worked out from that month&rsquo;s income, so the figure moves with it.</div>
                        <div>Averages {formatCurrency(expenses.averageMonthly, currencyCode)} a month across this plan.</div>
                        <div>{formatCurrency(expenses.fixedComponent, currencyCode)} + {percent(expenses.variableComponent)}% of income, where the line crosses.</div>
                        <div>
                            {expenses.dataPoints > 0
                                ? `Fitted from ${expenses.dataPoints} months · ${percent(expenses.rSquared)}% of the variation explained`
                                : "Not enough history to relate spending to income yet."}
                        </div>
                    </div>
                </Popover.Body>
            </Popover>
        }>
            <span className="expense-model-hint">
                {expenses.variableComponent > 0
                    // The slope as money rather than a rate: what an extra unit earned comes with
                    // in extra spending, which is the thing worth doing arithmetic against.
                    ? `moves about ${formatCurrency(expenses.variableComponent, currencyCode)} per ${formatCurrency(1, currencyCode)} earned`
                    : "not enough history to tie this to income"}
            </span>
        </OverlayTrigger>
    </div>
);

// Shown only when the plan's own income items fall materially short of the credits the accounts
// actually received. Spending is priced off the higher figure, so the forecast is pessimistic until
// the missing income is modelled — better said out loud than silently corrected for.
const IncomeShortfallNote: React.FC<{ expenses: ExpenseModel; currencyCode: string }> = ({ expenses, currencyCode }) => {
    if (expenses.modelledIncomeShortfall < 100) return null;

    return (
        <div className="forecast-notice">
            Your accounts receive about {formatCurrency(expenses.modelledIncomeShortfall, currencyCode)} a month
            more than this plan's income items account for. Expenses are modelled against the larger figure,
            so the outlook is gloomier than it should be until the rest of your income is listed.
        </div>
    );
};

export const ForecastOutlook: React.FC<ForecastOutlookProps> = ({ plan, summary, months, currencyCode, loading }) => {

    const onTrack = !summary || (summary.monthsBelowZero === 0 && summary.requiredMonthlyUplift <= 0);

    // Both income and spending move over the life of the plan -- income because it comes from dated
    // planned items, spending because it follows income -- so each is shown as the span it covers
    // rather than as one figure standing in for all of it.
    const incomes = months.map(m => m.incomeTotal);
    const outgoings = months.map(m => Math.abs(m.baselineOutgoingsTotal));

    const lowestBalanceRisk = !!summary && summary.lowestBalance < 0;
    const monthsRisk = !!summary && summary.monthsBelowZero > 0;
    const upliftRisk = !!summary && summary.requiredMonthlyUplift > 0;

    return (
        <div className="forecast-outlook">
            <div className="forecast-heading">
                {/* Kept as the real h2 so its font size and margins size the placeholder. */}
                <h2 className="forecast-title">{plan ? plan.name : <Skeleton.Text />}</h2>
                {plan?.startDate && plan?.endDate && (
                    <span className="forecast-period">
                        {format(parseISO(plan.startDate), "MMM yyyy")} – {format(parseISO(plan.endDate), "MMM yyyy")}
                    </span>
                )}
                {summary && (
                    <Badge pill muted bg={onTrack ? "success" : "warning"}>
                        {onTrack ? "On track" : "Needs attention"}
                    </Badge>
                )}
            </div>

            {/* Placeholder rather than nothing, so the strip holds its height until the numbers land. */}
            {!summary && <MetricsSkeleton className="forecast-metrics" count={5} />}

            {summary && (
                <div className="forecast-metrics">
                    <Kpi label="Monthly Income" tone="income">
                        <MonthlyRange months={incomes} currencyCode={currencyCode} tone="income" />
                        <Kpi.Sub>across the plan</Kpi.Sub>
                    </Kpi>
                    <Kpi label="Monthly Expenses" tone="expense">
                        <MonthlyRange months={outgoings} currencyCode={currencyCode} tone="expense" />
                        <ExpenseModelNote expenses={summary.expenses} currencyCode={currencyCode} />
                    </Kpi>
                    {/* A risk reads as an expense and a clean result as income — the same two
                        accents the rest of the app uses, rather than a second vocabulary. */}
                    <Kpi label="Lowest Balance" tone={lowestBalanceRisk ? "expense" : "income"}>
                        <Kpi.Value className={lowestBalanceRisk ? "negative" : undefined}>
                            <Amount amount={summary.lowestBalance} currencyCode={currencyCode} minus />
                        </Kpi.Value>
                        <Kpi.Sub>in {format(parseISO(summary.lowestBalanceMonth), "MMMM yyyy")}</Kpi.Sub>
                    </Kpi>
                    <Kpi label="Months Below Zero" tone={monthsRisk ? "expense" : "income"}>
                        <Kpi.Value className={monthsRisk ? "negative" : undefined}>{summary.monthsBelowZero}</Kpi.Value>
                        <Kpi.Sub>{summary.monthsBelowZero === 0 ? "never runs negative" : "needs attention"}</Kpi.Sub>
                    </Kpi>
                    <Kpi label="Required Monthly Uplift" tone={upliftRisk ? "expense" : "income"}>
                        <Kpi.Value className={upliftRisk ? "negative" : undefined}>
                            <Amount amount={summary.requiredMonthlyUplift} currencyCode={currencyCode} minus />
                        </Kpi.Value>
                        <Kpi.Sub>{summary.requiredMonthlyUplift > 0 ? "to avoid negative balance" : "no uplift required"}</Kpi.Sub>
                    </Kpi>
                </div>
            )}

            {summary && <IncomeShortfallNote expenses={summary.expenses} currencyCode={currencyCode} />}

            <Section header="Balance Projection" className="forecast-chart-section">
                <ForecastChart months={months} currencyCode={currencyCode} />
                {loading && <SpinnerContainer />}
            </Section>
        </div>
    );
};
