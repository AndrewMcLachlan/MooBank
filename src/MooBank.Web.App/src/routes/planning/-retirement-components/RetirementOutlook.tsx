import { Kpi, Section } from "@andrewmclachlan/moo-ds";
import { MetricsSkeleton } from "../-components/MetricsSkeleton";
import type { RetirementProjection } from "api/types.gen";
import { Amount } from "components";

interface RetirementOutlookProps {
    projection?: RetirementProjection;
    currencyCode: string;
    loading?: boolean;
}

export const RetirementOutlook: React.FC<RetirementOutlookProps> = ({ projection, currencyCode, loading }) => {

    // Four cards are always present and up to three more are conditional; five keeps the
    // placeholder close to the usual height without overshooting into an extra grid row.
    if (loading) return (
        <div className="retirement-outlook">
            <MetricsSkeleton className="retirement-metrics" count={5} />
        </div>
    );

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
                <Kpi label="Balance Today">
                    <Kpi.Value><Amount amount={summary.currentBalance} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                    <Kpi.Sub>across {projection.members.length === 1 ? "1 person" : `${projection.members.length} people`}</Kpi.Sub>
                </Kpi>
                <Kpi label="At Retirement">
                    <Kpi.Value><Amount amount={summary.balanceAtRetirement} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                    <Kpi.Sub>in {summary.retirementYear}</Kpi.Sub>
                </Kpi>
                <Kpi label="In Today's Dollars">
                    <Kpi.Value><Amount amount={summary.balanceAtRetirementInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                    <Kpi.Sub>what it would buy now</Kpi.Sub>
                </Kpi>
                <Kpi label="Sustainable Income">
                    <Kpi.Value><Amount amount={summary.sustainableIncomeInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                    <Kpi.Sub>the most this plan can pay to {summary.lifeExpectancyYear}, in today&rsquo;s dollars</Kpi.Sub>
                </Kpi>
                {summary.moneyRunsOutYear && (
                    <Kpi label="Money Runs Out">
                        <Kpi.Value>{summary.moneyRunsOutYear}</Kpi.Value>
                        <Kpi.Sub>before the plan&rsquo;s life expectancy of {summary.lifeExpectancyYear}</Kpi.Sub>
                    </Kpi>
                )}
                {!summary.moneyRunsOutYear && summary.finalBalanceInTodaysDollars > 0 && (
                    <Kpi label="Left Over">
                        <Kpi.Value><Amount amount={summary.finalBalanceInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                        <Kpi.Sub>at {summary.lifeExpectancyYear}, in today&rsquo;s dollars</Kpi.Sub>
                    </Kpi>
                )}
                {summary.totalPension > 0 && (
                    <Kpi label="Age Pension">
                        <Kpi.Value><Amount amount={summary.totalPension} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                        <Kpi.Sub>over the whole retirement</Kpi.Sub>
                    </Kpi>
                )}
                {summary.totalCosts > 0 && (
                    <Kpi label="Fees &amp; Insurance">
                        <Kpi.Value><Amount amount={summary.totalCosts} currencyCode={currencyCode} decimalPlaces={0} /></Kpi.Value>
                        <Kpi.Sub>over the whole projection</Kpi.Sub>
                    </Kpi>
                )}
            </div>
        </div>
    );
};
