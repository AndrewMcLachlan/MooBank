import { Section, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import type { ForecastMonth, ForecastSummary } from "api/types.gen";
import { Amount } from "components";
import { ForecastChart } from "./ForecastChart";

interface ForecastOutlookProps {
    summary?: ForecastSummary;
    months: ForecastMonth[];
    currencyCode: string;
    loading?: boolean;
}

export const ForecastOutlook: React.FC<ForecastOutlookProps> = ({ summary, months, currencyCode, loading }) => {

    const onTrack = !summary || (summary.monthsBelowZero === 0 && summary.requiredMonthlyUplift <= 0);

    const header = (
        <div className="outlook-header">
            <span>Outlook</span>
            {summary && (
                <span className={`health-pill ${onTrack ? "on-track" : "attention"}`}>
                    {onTrack ? "On track" : "Needs attention"}
                </span>
            )}
        </div>
    );

    const lowestBalanceRisk = !!summary && summary.lowestBalance < 0;
    const monthsRisk = !!summary && summary.monthsBelowZero > 0;
    const upliftRisk = !!summary && summary.requiredMonthlyUplift > 0;

    return (
        <Section header={header} className="forecast-outlook">
            {summary && (
                <div className="forecast-metrics">
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

            <ForecastChart months={months} currencyCode={currencyCode} />

            {summary && (
                <div className="outlook-footer">
                    <div className="outlook-totals">
                        Projected income <Amount amount={summary.totalIncome} currencyCode={currencyCode} /> · projected outgoings <Amount amount={summary.totalOutgoings} currencyCode={currencyCode} />
                    </div>
                    {summary.regression?.fellBackToFlatAverage && (
                        <div className="outlook-note">
                            Income–expense correlation was too weak (R² = {(summary.regression.rSquared * 100).toLocaleString(undefined, { maximumFractionDigits: 1 })}%) — using flat historical average instead.
                        </div>
                    )}
                </div>
            )}

            {loading && <SpinnerContainer />}
        </Section>
    );
};
