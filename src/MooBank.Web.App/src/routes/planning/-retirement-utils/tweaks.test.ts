import { describe, it, expect } from "vitest";
import type { RetirementPlan } from "api/types.gen";
import { applyDraftToPlan, emptyDraft, isDirty, isExcluded, memberValue, planTweakKeys, planValue, pruneDraft, withExcluded, withMemberValue, withPlanValue } from "./tweaks";

const selfId = "11111111-1111-1111-1111-111111111111";
const spouseId = "22222222-2222-2222-2222-222222222222";
const instrumentId = "33333333-3333-3333-3333-333333333333";
const selfUserId = "55555555-5555-5555-5555-555555555555";
const spouseUserId = "66666666-6666-6666-6666-666666666666";

const plan = (over: Partial<RetirementPlan> = {}): RetirementPlan => ({
    id: "44444444-4444-4444-4444-444444444444",
    name: "Retirement",
    expectedReturnRate: 0.065,
    inflationRate: 0.025,
    superGuaranteeRate: 0.12,
    contributionsTaxRate: 0.15,
    lifeExpectancy: 90,
    targetRetirementIncome: 60_000,
    cashBucketYears: 2,
    cashReturnRate: 0.03,
    createdUtc: "2026-01-01T00:00:00Z",
    updatedUtc: "2026-01-01T00:00:00Z",
    members: [
        { id: selfId, userId: selfUserId, name: "Self", currentAge: 47, currentIncome: 231_000, salarySacrifice: 5_000, retirementAge: 65, growthStrategy: "Balanced", annualFees: 372, insurancePremium: 364, instrumentIds: [instrumentId] },
        { id: spouseId, userId: spouseUserId, name: "Spouse", currentAge: 51, currentIncome: 18_000, salarySacrifice: 0, retirementAge: 67, growthStrategy: "Growth", annualFees: 250, insurancePremium: 0, instrumentIds: [] },
    ],
    ...over,
});

describe("an untouched draft", () => {
    it("is not dirty", () => {
        expect(isDirty(emptyDraft, plan())).toBe(false);
    });

    it("reads every value through to the plan", () => {
        const p = plan();

        expect(planValue(emptyDraft, p, "lifeExpectancy")).toBe(90);
        expect(memberValue(emptyDraft, p, selfId, "currentAge")).toBe(47);
        expect(memberValue(emptyDraft, p, selfId, "growthStrategy")).toBe("Balanced");
    });

    /**
     * The behaviour the sparse draft exists for: editing the plan's settings moves every slider
     * nobody has touched, rather than pinning them to what the plan held earlier.
     */
    it("follows the plan when the settings change underneath it", () => {
        const edited = plan({
            lifeExpectancy: 95,
            members: [
                { ...plan().members[0], currentAge: 48, currentIncome: 250_000 },
                plan().members[1],
            ],
        });

        expect(planValue(emptyDraft, edited, "lifeExpectancy")).toBe(95);
        expect(memberValue(emptyDraft, edited, selfId, "currentAge")).toBe(48);
        expect(memberValue(emptyDraft, edited, selfId, "currentIncome")).toBe(250_000);
    });
});

describe("moving a slider", () => {
    it("becomes dirty and overrides just that value", () => {
        const p = plan();
        const draft = withMemberValue(emptyDraft, p, selfId, "retirementAge", 60);

        expect(isDirty(draft, p)).toBe(true);
        expect(memberValue(draft, p, selfId, "retirementAge")).toBe(60);
        // Everything else still reads through.
        expect(memberValue(draft, p, selfId, "currentAge")).toBe(47);
        expect(memberValue(draft, p, spouseId, "retirementAge")).toBe(67);
    });

    it("clears the override when moved back to the plan's value", () => {
        const p = plan();
        const moved = withMemberValue(emptyDraft, p, selfId, "retirementAge", 60);
        const back = withMemberValue(moved, p, selfId, "retirementAge", 65);

        expect(isDirty(back, p)).toBe(false);
        expect(back.members).toHaveLength(0);
    });

    it("keeps a tweaked value when the settings change underneath it", () => {
        const p = plan();
        const draft = withMemberValue(emptyDraft, p, selfId, "retirementAge", 60);

        // Settings later raise the saved retirement age; the deliberate tweak wins.
        const edited = plan({ members: [{ ...p.members[0], retirementAge: 70 }, p.members[1]] });

        expect(memberValue(draft, edited, selfId, "retirementAge")).toBe(60);
    });

    it("tracks plan-level values the same way", () => {
        const p = plan();
        const draft = withPlanValue(emptyDraft, p, "lifeExpectancy", 100);

        expect(planValue(draft, p, "lifeExpectancy")).toBe(100);
        expect(isDirty(draft, p)).toBe(true);

        const back = withPlanValue(draft, p, "lifeExpectancy", 90);
        expect(isDirty(back, p)).toBe(false);
    });
});

describe("the target income slider", () => {
    it("overrides the plan and marks the draft dirty", () => {
        const p = plan();
        const draft = withPlanValue(emptyDraft, p, "targetRetirementIncome", 80_000);

        expect(planValue(draft, p, "targetRetirementIncome")).toBe(80_000);
        expect(isDirty(draft, p)).toBe(true);
    });

    it("folds into the plan on lock-in", () => {
        const p = plan();
        const draft = withPlanValue(emptyDraft, p, "targetRetirementIncome", 80_000);

        expect(applyDraftToPlan(draft, p).targetRetirementIncome).toBe(80_000);
    });

    /** The drawdown settings ride along untouched rather than being dropped on save. */
    it("carries the cash settings through untweaked", () => {
        const p = plan();
        const updated = applyDraftToPlan(emptyDraft, p);

        expect(updated.cashBucketYears).toBe(2);
        expect(updated.cashReturnRate).toBe(0.03);
    });
});

/**
 * The list backing both setting a value and the dirty check. It drifted once when a new plan-level
 * value was added to one place and not the other, leaving a slider unable to mark the draft dirty.
 */
describe("planTweakKeys", () => {
    it("marks the draft dirty for every key it lists", () => {
        const p = plan();

        for (const key of planTweakKeys) {
            const draft = { ...emptyDraft, [key]: 999 };
            expect(isDirty(draft, p), `${key} should mark the draft dirty`).toBe(true);
        }
    });
});

describe("leaving a member out of the projection", () => {
    it("marks them excluded and counts as a what-if", () => {
        const p = plan();
        const draft = withExcluded(emptyDraft, spouseId, true);

        expect(isExcluded(draft, spouseId)).toBe(true);
        expect(isExcluded(draft, selfId)).toBe(false);
        expect(isDirty(draft, p)).toBe(true);
    });

    it("puts them back", () => {
        const p = plan();
        const draft = withExcluded(withExcluded(emptyDraft, spouseId, true), spouseId, false);

        expect(isExcluded(draft, spouseId)).toBe(false);
        expect(isDirty(draft, p)).toBe(false);
    });

    it("does not exclude the same person twice", () => {
        const draft = withExcluded(withExcluded(emptyDraft, spouseId, true), spouseId, true);

        expect(draft.excludedMemberIds).toEqual([spouseId]);
    });

    /**
     * Seeing the plan without someone must never be a step towards removing them from it, so an
     * exclusion is not part of what locking in saves.
     */
    it("is never folded into the saved plan", () => {
        const p = plan();
        const draft = withExcluded(emptyDraft, spouseId, true);

        expect(applyDraftToPlan(draft, p).members).toHaveLength(2);
    });

    it("stops counting as a what-if once that member leaves the plan", () => {
        const p = plan();
        const draft = withExcluded(emptyDraft, spouseId, true);

        expect(isDirty(draft, plan({ members: [p.members[0]] }))).toBe(false);
    });
});

describe("a member removed from the plan", () => {
    it("has its override pruned", () => {
        const p = plan();
        const draft = withMemberValue(emptyDraft, p, spouseId, "retirementAge", 60);

        const withoutSpouse = plan({ members: [p.members[0]] });

        expect(pruneDraft(draft, withoutSpouse).members).toHaveLength(0);
        // And the page must not claim a what-if with nothing behind it.
        expect(isDirty(draft, withoutSpouse)).toBe(false);
    });
});

describe("applyDraftToPlan", () => {
    it("folds tweaks in and leaves everything else alone", () => {
        const p = plan();
        const draft = withPlanValue(
            withMemberValue(emptyDraft, p, selfId, "retirementAge", 60),
            p, "expectedReturnRate", 0.08);

        const updated = applyDraftToPlan(draft, p);
        const self = updated.members.find(m => m.id === selfId);

        expect(updated.expectedReturnRate).toBe(0.08);
        expect(self?.retirementAge).toBe(60);
        // Untweaked values, and the fields sliders never touch, survive.
        expect(self?.currentAge).toBe(47);
        expect(self?.name).toBe("Self");
        expect(self?.instrumentIds).toEqual([instrumentId]);
        expect(self?.annualFees).toBe(372);
    });

    it("round-trips an untouched draft to the same plan", () => {
        const p = plan();
        const updated = applyDraftToPlan(emptyDraft, p);

        expect(updated.expectedReturnRate).toBe(p.expectedReturnRate);
        expect(updated.members.map(m => m.retirementAge)).toEqual([65, 67]);
        expect(updated.members.map(m => m.insurancePremium)).toEqual([364, 0]);
    });
});
