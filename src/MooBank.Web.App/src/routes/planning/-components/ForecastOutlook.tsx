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

// The expenses figure. There is no single monthly expenses number — spending moves with income —
// so the two parts of the model lead, and the average is demoted to the caption it belongs in.
const ExpenseMetric: React.FC<{ expenses: ExpenseModel; currencyCode: string }> = ({ expenses, currencyCode }) => {
    if (expenses.usingFlatAverage) {
        return (
            <>
                <div className="eyebrow">Monthly Expenses</div>
                <div className="metric-value expense"><Amount amount={expenses.flatAverage} currencyCode={currencyCode} /></div>
                <div className="metric-sub">
                    <OverlayTrigger placement="bottom" overlay={
                        <Popover id="forecast-expense-popover">
                            <Popover.Body>
                                <div className="expense-model-detail">
                                    <div>Spending could not be tied to income from {expenses.dataPoints} month{expenses.dataPoints === 1 ? "" : "s"} of history.</div>
                                    <div>A flat average is being used instead, so a change in income will not move this figure.</div>
                                </div>
                            </Popover.Body>
                        </Popover>
                    }>
                        <span className="expense-model-hint">flat average · not tied to income</span>
                    </OverlayTrigger>
                </div>
            </>
        );
    }

    return (
        <>
            <div className="eyebrow">Monthly Expenses</div>
            <div className="metric-value expense">
                <Amount amount={expenses.fixedComponent} currencyCode={currencyCode} />
                <span className="expense-model-plus"> + {percent(expenses.variableComponent)}%</span>
            </div>
            <div className="metric-sub">
                <OverlayTrigger placement="bottom" overlay={
                    <Popover id="forecast-expense-popover">
                        <Popover.Body>
                            <div className="expense-model-detail">
                                <div>{formatCurrency(expenses.fixedComponent, currencyCode)} a month, plus {percent(expenses.variableComponent)}% of income.</div>
                                <div>Averages {formatCurrency(expenses.averageMonthly, currencyCode)} a month across this plan.</div>
                                <div>Fitted from {expenses.dataPoints} months · {percent(expenses.rSquared)}% R²</div>
                            </div>
                        </Popover.Body>
                    </Popover>
                }>
                    <span className="expense-model-hint">of income · averages {formatCurrency(expenses.averageMonthly, currencyCode)}</span>
                </OverlayTrigger>
            </div>
        </>
    );
};

// Shown only when the plan's own income items fall materially short of the credits the accounts
// actually received. Spending is priced off the higher figure, so the forecast is pessimistic until
// the missing income is modelled — better said out loud than silently corrected for.
const IncomeShortfallNote: React.FC<{ expenses: ExpenseModel; currencyCode: string }> = ({ expenses, currencyCode }) => {
    if (expenses.usingFlatAverage || expenses.modelledIncomeShortfall < 100) return null;

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

    // Income varies month to month now that it comes from planned items, so the headline figure is
    // an average rather than the single number a plan used to carry.
    const averageIncome = months.length > 0 ? months.reduce((sum, m) => sum + m.incomeTotal, 0) / months.length : 0;

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
                        <div className="metric-value income"><Amount amount={averageIncome} currencyCode={currencyCode} /></div>
                        <div className="metric-sub">average across the plan</div>
                    </Section>
                    <Section className="metric" data-tone="expense">
                        <ExpenseMetric expenses={summary.expenses} currencyCode={currencyCode} />
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
