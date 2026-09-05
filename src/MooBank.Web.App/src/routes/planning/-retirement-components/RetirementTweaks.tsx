import { Button, Section } from "@andrewmclachlan/moo-ds";
import type { GrowthStrategy, RetirementPlan, RetirementProjectionOverrides, RetirementProjectionSummary } from "api/types.gen";
import { formatCurrency } from "utils/currency";
import { TweakSlider } from "./TweakSlider";
import { isDirty, isExcluded, memberValue, planValue, withExcluded, withMemberValue, withPlanValue, type PlanTweakKey } from "../-retirement-utils/tweaks";
import { growthStrategies, minWorkingAge, toPercent } from "../-retirement-utils/retirementDefaults";

interface RetirementTweaksProps {
    plan: RetirementPlan;
    draft: RetirementProjectionOverrides;
    /** The projection the sliders are sitting on, which is where the sync figures come from. */
    summary?: RetirementProjectionSummary;
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
 *
 * Each slider reads its value through the draft to the plan, so a slider nobody has moved follows
 * the plan — including after the settings are edited underneath it.
 */
export const RetirementTweaks: React.FC<RetirementTweaksProps> = ({ plan, draft, summary, saving, currencyCode, onChange, onReset, onLockIn }) => {

    const money = (value: number) => formatCurrency(value, currencyCode, 0);
    const dirty = isDirty(draft, plan);

    const setPlan = <K extends PlanTweakKey>(key: K, value: number) =>
        onChange(withPlanValue(draft, plan, key, value));

    /**
     * Every slider moves its own value and nothing else. What a change costs — whether the money
     * still lasts, and how long — comes back with the next projection, which is the only thing
     * that knows.
     */
    const setLifeExpectancy = (age: number) => onChange(withPlanValue(draft, plan, "lifeExpectancy", age));

    const setTargetIncome = (income: number) => onChange(withPlanValue(draft, plan, "targetRetirementIncome", income));

    return (
        <Section header="Try It Out">
            <p className="retirement-tweak-note">
                {dirty
                    ? "Showing a what-if. Lock it in to save these to the plan, or reset to go back — either way it is lost on refresh."
                    : "Move a slider to see what changes. Nothing is saved until you lock it in."}
            </p>

            {plan.members.map(member => {
                const set = <K extends "currentAge" | "currentIncome" | "salarySacrifice" | "retirementAge" | "growthStrategy">(key: K, value: unknown) =>
                    onChange(withMemberValue(draft, plan, member.id, key, value as never));

                const age = memberValue(draft, plan, member.id, "currentAge") as number;
                const retirementAge = memberValue(draft, plan, member.id, "retirementAge") as number;
                const income = memberValue(draft, plan, member.id, "currentIncome") as number;
                const sacrifice = memberValue(draft, plan, member.id, "salarySacrifice") as number;
                const strategy = memberValue(draft, plan, member.id, "growthStrategy") as GrowthStrategy;
                const excluded = isExcluded(draft, member.id);

                return (
                    <div className={excluded ? "tweak-member tweak-member-excluded" : "tweak-member"} key={member.id}>
                        <div className="tweak-member-header">
                            <h4 className="tweak-member-name">{member.name || "Unnamed"}</h4>
                            {/* Only worth offering for a household — there is nothing to see in a
                                plan of one person without that person. */}
                            {plan.members.length > 1 && (
                                <label className="tweak-member-include">
                                    <input
                                        type="checkbox"
                                        checked={!excluded}
                                        onChange={e => onChange(withExcluded(draft, member.id, !e.target.checked))}
                                    />
                                    Include
                                </label>
                            )}
                        </div>
                        <div className="tweak-grid">
                            <TweakSlider
                                label="Retirement age"
                                value={retirementAge}
                                min={Math.min(age, 50)}
                                max={80}
                                display={`${retirementAge}`}
                                savedDisplay={retirementAge !== member.retirementAge ? `${member.retirementAge}` : undefined}
                                onChange={v => set("retirementAge", v)}
                            />
                            <TweakSlider
                                label="Current age"
                                value={age}
                                min={minWorkingAge}
                                max={80}
                                display={`${age}`}
                                savedDisplay={age !== member.currentAge ? `${member.currentAge}` : undefined}
                                onChange={v => set("currentAge", v)}
                            />
                            <TweakSlider
                                label="Income"
                                value={income}
                                min={0}
                                max={400_000}
                                step={1_000}
                                display={money(income)}
                                savedDisplay={income !== member.currentIncome ? money(member.currentIncome) : undefined}
                                onChange={v => set("currentIncome", v)}
                            />
                            <TweakSlider
                                label="Salary sacrifice"
                                value={sacrifice}
                                min={0}
                                max={30_000}
                                step={500}
                                display={money(sacrifice)}
                                savedDisplay={sacrifice !== member.salarySacrifice ? money(member.salarySacrifice) : undefined}
                                onChange={v => set("salarySacrifice", v)}
                            />
                            <label className="tweak-select">
                                <span className="tweak-slider-label">Growth strategy</span>
                                <select
                                    className="form-control"
                                    value={strategy}
                                    onChange={e => set("growthStrategy", e.currentTarget.value as GrowthStrategy)}
                                >
                                    {growthStrategies.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
                                </select>
                                {strategy !== member.growthStrategy && (
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
                        value={toPercent(planValue(draft, plan, "expectedReturnRate"))}
                        min={0}
                        max={15}
                        step={0.1}
                        display={`${toPercent(planValue(draft, plan, "expectedReturnRate"))}%`}
                        savedDisplay={draft.expectedReturnRate != null ? `${toPercent(plan.expectedReturnRate)}%` : undefined}
                        onChange={percent => setPlan("expectedReturnRate", percent / 100)}
                    />
                    <TweakSlider
                        label="Inflation"
                        value={toPercent(planValue(draft, plan, "inflationRate"))}
                        min={0}
                        max={10}
                        step={0.1}
                        display={`${toPercent(planValue(draft, plan, "inflationRate"))}%`}
                        savedDisplay={draft.inflationRate != null ? `${toPercent(plan.inflationRate)}%` : undefined}
                        onChange={percent => setPlan("inflationRate", percent / 100)}
                    />
                    <TweakSlider
                        label="Target income"
                        value={planValue(draft, plan, "targetRetirementIncome")}
                        min={0}
                        max={200_000}
                        step={1_000}
                        display={money(planValue(draft, plan, "targetRetirementIncome"))}
                        savedDisplay={draft.targetRetirementIncome != null ? money(plan.targetRetirementIncome) : undefined}
                        onChange={setTargetIncome}
                    />
                    <TweakSlider
                        label="Savings must last until"
                        value={planValue(draft, plan, "lifeExpectancy")}
                        min={70}
                        max={110}
                        display={`age ${planValue(draft, plan, "lifeExpectancy")}`}
                        savedDisplay={draft.lifeExpectancy != null ? `age ${plan.lifeExpectancy}` : undefined}
                        onChange={setLifeExpectancy}
                    />
                </div>
                <p className="retirement-tweak-note">
                    Expected return applies to anyone on the Custom strategy; the named strategies carry their own.
                    Target income is what the household draws each year once everyone has retired, in today's dollars.
                    Set it above what the savings can carry and the money runs out before the age beside it, leaving the
                    Age Pension to pay from there — a plan, if that is the one you want, not a mistake to be corrected.
                    Clearing "Include" leaves someone out of the projection to see the plan for one person; it changes
                    what is shown, never who is on the plan, so locking in cannot remove them.
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
