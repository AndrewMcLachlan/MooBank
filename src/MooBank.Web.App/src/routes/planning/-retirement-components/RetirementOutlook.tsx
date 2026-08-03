import { Section } from "@andrewmclachlan/moo-ds";
import { MetricsSkeleton } from "../-components/MetricsSkeleton";
import type { RetirementProjection } from "api/types.gen";
import { Amount, Kpi, KpiSub, KpiValue } from "components";

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
                    <KpiValue><Amount amount={summary.currentBalance} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                    <KpiSub>across {projection.members.length === 1 ? "1 person" : `${projection.members.length} people`}</KpiSub>
                </Kpi>
                <Kpi label="At Retirement">
                    <KpiValue><Amount amount={summary.balanceAtRetirement} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                    <KpiSub>in {summary.retirementYear}</KpiSub>
                </Kpi>
                <Kpi label="In Today's Dollars">
                    <KpiValue><Amount amount={summary.balanceAtRetirementInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                    <KpiSub>what it would buy now</KpiSub>
                </Kpi>
                <Kpi label="Sustainable Income">
                    <KpiValue><Amount amount={summary.sustainableIncomeInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                    <KpiSub>the most this plan can pay to {summary.lifeExpectancyYear}, in today&rsquo;s dollars</KpiSub>
                </Kpi>
                {summary.moneyRunsOutYear && (
                    <Kpi label="Money Runs Out">
                        <KpiValue>{summary.moneyRunsOutYear}</KpiValue>
                        <KpiSub>before the plan&rsquo;s life expectancy of {summary.lifeExpectancyYear}</KpiSub>
                    </Kpi>
                )}
                {!summary.moneyRunsOutYear && summary.finalBalanceInTodaysDollars > 0 && (
                    <Kpi label="Left Over">
                        <KpiValue><Amount amount={summary.finalBalanceInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                        <KpiSub>at {summary.lifeExpectancyYear}, in today&rsquo;s dollars</KpiSub>
                    </Kpi>
                )}
                {summary.totalPension > 0 && (
                    <Kpi label="Age Pension">
                        <KpiValue><Amount amount={summary.totalPension} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                        <KpiSub>over the whole retirement</KpiSub>
                    </Kpi>
                )}
                {summary.totalCosts > 0 && (
                    <Kpi label="Fees &amp; Insurance">
                        <KpiValue><Amount amount={summary.totalCosts} currencyCode={currencyCode} decimalPlaces={0} /></KpiValue>
                        <KpiSub>over the whole projection</KpiSub>
                    </Kpi>
                )}
            </div>
        </div>
    );
};
