import { Badge, OverlayTrigger, Popover, Section, SpinnerContainer } from "@andrewmclachlan/moo-ds";
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
        <div className={`metric-value ${tone}`}>
            <Amount amount={low} currencyCode={currencyCode} />
            {Math.round(high - low) > 0 && (
                <>
                    <span className="metric-range-to">to</span>
                    <Amount amount={high} currencyCode={currencyCode} />
                </>
            )}
        </div>
    );
};

// The model behind the expenses span: a fixed amount plus a share of income. This is what says how
// the figure moves when income changes, which no amount on its own can.
const ExpenseModelNote: React.FC<{ expenses: ExpenseModel; currencyCode: string }> = ({ expenses, currencyCode }) => (
    <div className="metric-sub">
        <OverlayTrigger placement="bottom" overlay={
            <Popover id="forecast-expense-popover">
                <Popover.Body>
                    <div className="expense-model-detail">
                        <div>Each month is worked out from that month&rsquo;s income, so the figure moves with it.</div>
                        <div>Averages {formatCurrency(expenses.averageMonthly, currencyCode)} a month across this plan.</div>
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
                    ? `${formatCurrency(expenses.fixedComponent, currencyCode)} + ${percent(expenses.variableComponent)}% of income`
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
                <h2 className="forecast-title">{plan?.name}</h2>
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

            {summary && (
                <div className="forecast-metrics">
                    <Section className="metric" data-tone="income">
                        <div className="eyebrow">Monthly Income</div>
                        <MonthlyRange months={incomes} currencyCode={currencyCode} tone="income" />
                        <div className="metric-sub">across the plan</div>
                    </Section>
                    <Section className="metric" data-tone="expense">
                        <div className="eyebrow">Monthly Expenses</div>
                        <MonthlyRange months={outgoings} currencyCode={currencyCode} tone="expense" />
                        <ExpenseModelNote expenses={summary.expenses} currencyCode={currencyCode} />
                    </Section>
                    <Section className="metric" data-tone={lowestBalanceRisk ? "risk" : "ok"}>
                        <div className="eyebrow">Lowest Balance</div>
                        <div className={`metric-value ${lowestBalanceRisk ? "negative" : ""}`}>
                            <Amount amount={summary.lowestBalance} currencyCode={currencyCode} minus />
                        </div>
                        <div className="metric-sub">in {format(parseISO(summary.lowestBalanceMonth), "MMMM yyyy")}</div>
                    </Section>
                    <Section className="metric" data-tone={monthsRisk ? "risk" : "ok"}>
                        <div className="eyebrow">Months Below Zero</div>
                        <div className={`metric-value ${monthsRisk ? "negative" : ""}`}>{summary.monthsBelowZero}</div>
                        <div className="metric-sub">{summary.monthsBelowZero === 0 ? "never runs negative" : "needs attention"}</div>
                    </Section>
                    <Section className="metric" data-tone={upliftRisk ? "risk" : "ok"}>
                        <div className="eyebrow">Required Monthly Uplift</div>
                        <div className={`metric-value ${upliftRisk ? "negative" : ""}`}>
                            <Amount amount={summary.requiredMonthlyUplift} currencyCode={currencyCode} minus />
                        </div>
                        <div className="metric-sub">{summary.requiredMonthlyUplift > 0 ? "to avoid negative balance" : "no uplift required"}</div>
                    </Section>
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
