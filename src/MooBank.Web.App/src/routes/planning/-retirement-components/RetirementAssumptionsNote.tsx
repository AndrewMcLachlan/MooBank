import { Section } from "@andrewmclachlan/moo-ds";
import type { RetirementPlan } from "api/types.gen";
import { toPercent } from "../-retirement-utils/retirementDefaults";

interface RetirementAssumptionsNoteProps {
    plan?: RetirementPlan;
}

/**
 * States what the projection does and does not account for. The figures are arithmetic on the
 * stated assumptions, and anyone reading them should be able to see what those are without
 * opening the settings.
 */
export const RetirementAssumptionsNote: React.FC<RetirementAssumptionsNoteProps> = ({ plan }) => {

    if (!plan) return null;

    return (
        <Section header="Assumptions">
            <dl className="retirement-assumption-list">
                <div>
                    <dt>Expected return</dt>
                    <dd>{toPercent(plan.expectedReturnRate)}% a year</dd>
                </div>
                <div>
                    <dt>Inflation</dt>
                    <dd>{toPercent(plan.inflationRate)}% a year</dd>
                </div>
                <div>
                    <dt>Employer contribution</dt>
                    <dd>{toPercent(plan.superGuaranteeRate)}% of income</dd>
                </div>
                <div>
                    <dt>Contributions tax</dt>
                    <dd>{toPercent(plan.contributionsTaxRate)}%</dd>
                </div>
                <div>
                    <dt>Savings must last until</dt>
                    <dd>age {plan.lifeExpectancy}</dd>
                </div>
                <div>
                    <dt>Cash bucket</dt>
                    <dd>{plan.cashBucketYears} {plan.cashBucketYears === 1 ? "year" : "years"} of spending</dd>
                </div>
            </dl>
            <p className="retirement-caveat">
                Income, salary sacrifice, fund fees and insurance premiums all grow with inflation, and a year's
                return is applied to the opening balance only. Fees and premiums come out year by year, so they
                cost their own value plus the growth they would have earned. Premiums keep being charged after
                retirement, which is the conservative reading — cover usually ceases.
            </p>
            <p className="retirement-caveat">
                Investments follow a <strong>cash bucket</strong>: a few years of spending are held in cash so a
                market fall never forces units to be sold cheaply to live on, while the rest stays in each person's
                growth strategy. It is the common answer to sequencing risk — the danger of a bad year just as you
                start drawing — without giving up the returns a retirement of twenty or thirty years needs to
                outrun inflation. Moving a whole balance to cash trades one risk for the other.
            </p>
            <p className="retirement-caveat">
                The Age Pension is modelled on the assets test only, using the rates in settings. Contribution caps,
                tax on earnings within the fund and tax on withdrawals are not modelled. These figures are arithmetic
                on the assumptions above, not a prediction or financial advice.
            </p>
        </Section>
    );
};
