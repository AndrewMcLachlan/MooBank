import { Badge, OverlayTrigger, Popover, Section, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import type { ForecastMonth, ForecastPlan, ForecastSummary } from "api/types.gen";
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

// The expense calc note. When the plan is income-correlated and the regression held, it exposes the
// fitted model (fixed/variable/R²) in a hover popover — carried over from the old settings panel.
const ExpenseNote: React.FC<{ plan?: ForecastPlan; summary: ForecastSummary; currencyCode: string }> = ({ plan, summary, currencyCode }) => {
    if (plan?.outgoingStrategy?.mode !== "IncomeCorrelated") {
        return <>historical average</>;
    }
    const regression = summary.regression;
    if (!regression || regression.fellBackToFlatAverage) {
        return <>income-correlated · flat average</>;
    }
    return (
        <OverlayTrigger placement="bottom" overlay={
            <Popover id="forecast-regression-popover">
                <Popover.Body>
                    <div className="regression-popover">
                        <div>Fixed expenses: {formatCurrency(regression.fixedComponent, currencyCode)}/mo</div>
                        <div>Variable rate: {(regression.variableComponent * 100).toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 })}% of income</div>
                        <div>Model fit: {(regression.rSquared * 100).toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 })}% R²</div>
                    </div>
                </Popover.Body>
            </Popover>
        }>
            <span className="regression-hint">income-correlated</span>
        </OverlayTrigger>
    );
};

export const ForecastOutlook: React.FC<ForecastOutlookProps> = ({ plan, summary, months, currencyCode, loading }) => {

    const onTrack = !summary || (summary.monthsBelowZero === 0 && summary.requiredMonthlyUplift <= 0);

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
                        <div className="metric-value income"><Amount amount={plan?.incomeStrategy?.manualRecurring?.amount ?? 0} currencyCode={currencyCode} /></div>
                        <div className="metric-sub">per month</div>
                    </Section>
                    <Section className="metric" data-tone="expense">
                        <div className="eyebrow">Monthly Expenses</div>
                        <div className="metric-value expense"><Amount amount={summary.monthlyBaselineOutgoings} currencyCode={currencyCode} /></div>
                        <div className="metric-sub"><ExpenseNote plan={plan} summary={summary} currencyCode={currencyCode} /></div>
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

            <Section header="Balance Projection" className="forecast-chart-section">
                <ForecastChart months={months} currencyCode={currencyCode} />
                {loading && <SpinnerContainer />}
            </Section>
        </div>
    );
};
