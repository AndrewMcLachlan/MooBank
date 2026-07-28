import { Section } from "@andrewmclachlan/moo-ds";
import type { RetirementPlan } from "api/types.gen";
import { toPercent } from "../-utils/retirementDefaults";

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
            </dl>
            <p className="retirement-caveat">
                Income is assumed to grow with inflation, and a year's return is applied to the opening balance
                only. Salary sacrifice, contribution caps, fund fees, insurance premiums, tax on earnings within
                the fund, the Age Pension and tax on withdrawals are not modelled. These figures are arithmetic
                on the assumptions above, not a prediction or financial advice.
            </p>
        </Section>
    );
};
