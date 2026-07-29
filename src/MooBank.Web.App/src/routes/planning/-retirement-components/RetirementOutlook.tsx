import { Section, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import type { RetirementProjection } from "api/types.gen";
import { Amount } from "components";

interface RetirementOutlookProps {
    projection?: RetirementProjection;
    currencyCode: string;
    loading?: boolean;
}

export const RetirementOutlook: React.FC<RetirementOutlookProps> = ({ projection, currencyCode, loading }) => {

    if (loading) return <SpinnerContainer />;

    if (!projection || projection.members.length === 0) {
        return (
            <Section header="Outlook">
                <p className="retirement-empty">Add someone to the plan and pick their superannuation accounts to see a projection.</p>
            </Section>
        );
    }

    const { summary } = projection;

    return (
        <div className="retirement-outlook">
            <div className="retirement-metrics">
                <Section className="metric">
                    <div className="eyebrow">Balance Today</div>
                    <div className="metric-value"><Amount amount={summary.currentBalance} currencyCode={currencyCode} decimalPlaces={0} /></div>
                    <div className="metric-sub">across {projection.members.length === 1 ? "1 person" : `${projection.members.length} people`}</div>
                </Section>
                <Section className="metric">
                    <div className="eyebrow">At Retirement</div>
                    <div className="metric-value"><Amount amount={summary.balanceAtRetirement} currencyCode={currencyCode} decimalPlaces={0} /></div>
                    <div className="metric-sub">in {summary.retirementYear}</div>
                </Section>
                <Section className="metric">
                    <div className="eyebrow">In Today's Dollars</div>
                    <div className="metric-value"><Amount amount={summary.balanceAtRetirementInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></div>
                    <div className="metric-sub">what it would buy now</div>
                </Section>
                <Section className="metric">
                    <div className="eyebrow">Retirement Income</div>
                    <div className="metric-value"><Amount amount={summary.annualRetirementIncomeInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></div>
                    <div className="metric-sub">a year, in today's dollars</div>
                </Section>
                {summary.moneyRunsOutYear && (
                    <Section className="metric metric-warning">
                        <div className="eyebrow">Money Runs Out</div>
                        <div className="metric-value">{summary.moneyRunsOutYear}</div>
                        <div className="metric-sub">before the plan's life expectancy of {summary.lifeExpectancyYear}</div>
                    </Section>
                )}
                {!summary.moneyRunsOutYear && summary.finalBalanceInTodaysDollars > 0 && (
                    <Section className="metric">
                        <div className="eyebrow">Left Over</div>
                        <div className="metric-value"><Amount amount={summary.finalBalanceInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></div>
                        <div className="metric-sub">at {summary.lifeExpectancyYear}, in today's dollars</div>
                    </Section>
                )}
                {summary.totalCosts > 0 && (
                    <Section className="metric">
                        <div className="eyebrow">Fees &amp; Insurance</div>
                        <div className="metric-value"><Amount amount={summary.totalCosts} currencyCode={currencyCode} decimalPlaces={0} /></div>
                        <div className="metric-sub">over the whole projection</div>
                    </Section>
                )}
            </div>
        </div>
    );
};
