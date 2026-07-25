import { Section, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import type { ForecastMonth, ForecastPlan, ForecastSummary } from "api/types.gen";
import { Amount } from "components";
import { ForecastChart } from "./ForecastChart";

interface ForecastOutlookProps {
    plan?: ForecastPlan;
    summary?: ForecastSummary;
    months: ForecastMonth[];
    currencyCode: string;
    loading?: boolean;
}

const expenseNote = (plan?: ForecastPlan, summary?: ForecastSummary): string => {
    if (plan?.outgoingStrategy?.mode !== "IncomeCorrelated") {
        return "historical average";
    }
    return summary?.regression && !summary.regression.fellBackToFlatAverage
        ? "income-correlated"
        : "income-correlated · flat average";
};

export const ForecastOutlook: React.FC<ForecastOutlookProps> = ({ plan, summary, months, currencyCode, loading }) => {

    const onTrack = !summary || (summary.monthsBelowZero === 0 && summary.requiredMonthlyUplift <= 0);

    const lowestBalanceRisk = !!summary && summary.lowestBalance < 0;
    const monthsRisk = !!summary && summary.monthsBelowZero > 0;
    const upliftRisk = !!summary && summary.requiredMonthlyUplift > 0;

    return (
        <div className="forecast-outlook">
            <div className="forecast-heading">
                <span className="forecast-title">{plan?.name}</span>
                {plan?.startDate && plan?.endDate && (
                    <span className="forecast-period">
                        {format(parseISO(plan.startDate), "MMM yyyy")} – {format(parseISO(plan.endDate), "MMM yyyy")}
                    </span>
                )}
                {summary && (
                    <span className={`health-pill ${onTrack ? "on-track" : "attention"}`}>
                        {onTrack ? "On track" : "Needs attention"}
                    </span>
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
                        <div className="metric-sub">{expenseNote(plan, summary)}</div>
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
