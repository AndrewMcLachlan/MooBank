import { Button, Section } from "@andrewmclachlan/moo-ds";
import type { GrowthStrategy, RetirementPlan, RetirementProjectionOverrides } from "api/types.gen";
import { formatCurrency } from "utils/currency";
import { TweakSlider } from "./TweakSlider";
import { withMember } from "../-retirement-utils/tweaks";
import { growthStrategies, toPercent } from "../-retirement-utils/retirementDefaults";

interface RetirementTweaksProps {
    plan: RetirementPlan;
    draft: RetirementProjectionOverrides;
    dirty: boolean;
    saving: boolean;
    currencyCode: string;
    onChange: (draft: RetirementProjectionOverrides) => void;
    onReset: () => void;
    onLockIn: () => void;
}

/**
 * Try-it-out controls for the projection.
 *
 * Nothing here is saved. Moving a slider re-runs the projection under the new value and leaves the
 * plan alone, so a refresh returns to the saved position. "Lock in" is the only thing that writes.
 */
export const RetirementTweaks: React.FC<RetirementTweaksProps> = ({ plan, draft, dirty, saving, currencyCode, onChange, onReset, onLockIn }) => {

    const money = (value: number) => formatCurrency(value, currencyCode, 0);

    return (
        <Section header="Try It Out">
            <p className="retirement-tweak-note">
                {dirty
                    ? "Showing a what-if. Lock it in to save these to the plan, or reset to go back — either way it is lost on refresh."
                    : "Move a slider to see what changes. Nothing is saved until you lock it in."}
            </p>

            {plan.members.map(member => {
                const tweak = draft.members.find(m => m.memberId === member.id);
                if (!tweak) return null;

                const set = (changes: Partial<typeof tweak>) => onChange(withMember(draft, member.id, changes));

                return (
                    <div className="tweak-member" key={member.id}>
                        <h4 className="tweak-member-name">{member.name}</h4>
                        <div className="tweak-grid">
                            <TweakSlider
                                label="Retirement age"
                                value={tweak.retirementAge ?? member.retirementAge}
                                min={Math.min(tweak.currentAge ?? member.currentAge, 50)}
                                max={80}
                                display={`${tweak.retirementAge}`}
                                savedDisplay={tweak.retirementAge !== member.retirementAge ? `${member.retirementAge}` : undefined}
                                onChange={retirementAge => set({ retirementAge })}
                            />
                            <TweakSlider
                                label="Current age"
                                value={tweak.currentAge ?? member.currentAge}
                                min={16}
                                max={80}
                                display={`${tweak.currentAge}`}
                                savedDisplay={tweak.currentAge !== member.currentAge ? `${member.currentAge}` : undefined}
                                onChange={currentAge => set({ currentAge })}
                            />
                            <TweakSlider
                                label="Income"
                                value={tweak.currentIncome ?? member.currentIncome}
                                min={0}
                                max={400_000}
                                step={1_000}
                                display={money(tweak.currentIncome ?? 0)}
                                savedDisplay={tweak.currentIncome !== member.currentIncome ? money(member.currentIncome) : undefined}
                                onChange={currentIncome => set({ currentIncome })}
                            />
                            <TweakSlider
                                label="Salary sacrifice"
                                value={tweak.salarySacrifice ?? member.salarySacrifice}
                                min={0}
                                max={30_000}
                                step={500}
                                display={money(tweak.salarySacrifice ?? 0)}
                                savedDisplay={tweak.salarySacrifice !== member.salarySacrifice ? money(member.salarySacrifice) : undefined}
                                onChange={salarySacrifice => set({ salarySacrifice })}
                            />
                            <label className="tweak-select">
                                <span className="tweak-slider-label">Growth strategy</span>
                                <select
                                    className="form-control"
                                    value={tweak.growthStrategy ?? member.growthStrategy}
                                    onChange={e => set({ growthStrategy: e.currentTarget.value as GrowthStrategy })}
                                >
                                    {growthStrategies.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
                                </select>
                                {tweak.growthStrategy !== member.growthStrategy && (
                                    <span className="tweak-slider-saved">saved: {growthStrategies.find(s => s.value === member.growthStrategy)?.label}</span>
                                )}
                            </label>
                        </div>
                    </div>
                );
            })}

            <div className="tweak-member">
                <h4 className="tweak-member-name">Whole plan</h4>
                <div className="tweak-grid">
                    <TweakSlider
                        label="Expected return"
                        value={toPercent(draft.expectedReturnRate ?? plan.expectedReturnRate)}
                        min={0}
                        max={15}
                        step={0.1}
                        display={`${toPercent(draft.expectedReturnRate ?? 0)}%`}
                        savedDisplay={draft.expectedReturnRate !== plan.expectedReturnRate ? `${toPercent(plan.expectedReturnRate)}%` : undefined}
                        onChange={percent => onChange({ ...draft, expectedReturnRate: percent / 100 })}
                    />
                    <TweakSlider
                        label="Inflation"
                        value={toPercent(draft.inflationRate ?? plan.inflationRate)}
                        min={0}
                        max={10}
                        step={0.1}
                        display={`${toPercent(draft.inflationRate ?? 0)}%`}
                        savedDisplay={draft.inflationRate !== plan.inflationRate ? `${toPercent(plan.inflationRate)}%` : undefined}
                        onChange={percent => onChange({ ...draft, inflationRate: percent / 100 })}
                    />
                    <TweakSlider
                        label="Savings must last until"
                        value={draft.lifeExpectancy ?? plan.lifeExpectancy}
                        min={70}
                        max={110}
                        display={`age ${draft.lifeExpectancy}`}
                        savedDisplay={draft.lifeExpectancy !== plan.lifeExpectancy ? `age ${plan.lifeExpectancy}` : undefined}
                        onChange={lifeExpectancy => onChange({ ...draft, lifeExpectancy })}
                    />
                </div>
                <p className="retirement-tweak-note">
                    Expected return applies to anyone on the Custom strategy; the named strategies carry their own.
                </p>
            </div>

            {dirty && (
                <div className="tweak-actions">
                    <Button variant="outline-primary" onClick={onReset}>Reset</Button>
                    <Button variant="primary" onClick={onLockIn} disabled={saving}>Lock In</Button>
                </div>
            )}
        </Section>
    );
};
