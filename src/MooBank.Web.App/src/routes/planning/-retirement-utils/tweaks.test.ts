import { describe, it, expect } from "vitest";
import type { RetirementPlan } from "api/types.gen";
import { applyDraftToPlan, draftFromPlan, isDirty, withMember } from "./tweaks";

const selfId = "11111111-1111-1111-1111-111111111111";
const spouseId = "22222222-2222-2222-2222-222222222222";
const instrumentId = "33333333-3333-3333-3333-333333333333";

const plan = (): RetirementPlan => ({
    id: "44444444-4444-4444-4444-444444444444",
    name: "Retirement",
    expectedReturnRate: 0.065,
    inflationRate: 0.025,
    superGuaranteeRate: 0.12,
    contributionsTaxRate: 0.15,
    lifeExpectancy: 90,
    createdUtc: "2026-01-01T00:00:00Z",
    updatedUtc: "2026-01-01T00:00:00Z",
    members: [
        { id: selfId, name: "Self", currentAge: 45, currentIncome: 120_000, salarySacrifice: 5_000, retirementAge: 65, growthStrategy: "Balanced", annualFees: 372, insurancePremium: 364, instrumentIds: [instrumentId] },
        { id: spouseId, name: "Spouse", currentAge: 43, currentIncome: 90_000, salarySacrifice: 0, retirementAge: 67, growthStrategy: "Growth", annualFees: 250, insurancePremium: 0, instrumentIds: [] },
    ],
});

describe("draftFromPlan", () => {
    it("seeds every slider from the saved plan", () => {
        const draft = draftFromPlan(plan());

        expect(draft.expectedReturnRate).toBe(0.065);
        expect(draft.lifeExpectancy).toBe(90);
        expect(draft.members).toHaveLength(2);

        const self = draft.members.find(m => m.memberId === selfId);
        expect(self).toMatchObject({ currentAge: 45, currentIncome: 120_000, salarySacrifice: 5_000, retirementAge: 65, growthStrategy: "Balanced" });
    });

    it("is not dirty before anything moves", () => {
        const p = plan();
        expect(isDirty(draftFromPlan(p), p)).toBe(false);
    });
});

describe("withMember", () => {
    it("changes only the named member", () => {
        const p = plan();
        const draft = withMember(draftFromPlan(p), selfId, { retirementAge: 60 });

        expect(draft.members.find(m => m.memberId === selfId)?.retirementAge).toBe(60);
        expect(draft.members.find(m => m.memberId === spouseId)?.retirementAge).toBe(67);
    });

    it("marks the draft dirty", () => {
        const p = plan();
        expect(isDirty(withMember(draftFromPlan(p), selfId, { retirementAge: 60 }), p)).toBe(true);
    });

    it("is clean again once the value is moved back", () => {
        const p = plan();
        const moved = withMember(draftFromPlan(p), selfId, { retirementAge: 60 });
        const restored = withMember(moved, selfId, { retirementAge: 65 });

        expect(isDirty(restored, p)).toBe(false);
    });
});

describe("applyDraftToPlan", () => {
    it("folds tweaked values back onto the plan", () => {
        const p = plan();
        const draft = withMember({ ...draftFromPlan(p), expectedReturnRate: 0.08 }, selfId, { retirementAge: 60, salarySacrifice: 20_000 });

        const updated = applyDraftToPlan(draft, p);

        expect(updated.expectedReturnRate).toBe(0.08);

        const self = updated.members.find(m => m.id === selfId);
        expect(self?.retirementAge).toBe(60);
        expect(self?.salarySacrifice).toBe(20_000);
    });

    it("keeps what the sliders do not touch", () => {
        const p = plan();
        const draft = withMember(draftFromPlan(p), selfId, { retirementAge: 60 });

        const self = applyDraftToPlan(draft, p).members.find(m => m.id === selfId);

        // Name and superannuation accounts are only editable in settings, so a lock-in must not
        // drop them.
        expect(self?.name).toBe("Self");
        expect(self?.instrumentIds).toEqual([instrumentId]);
    });

    it("leaves untweaked members exactly as they were", () => {
        const p = plan();
        const draft = withMember(draftFromPlan(p), selfId, { retirementAge: 60 });

        const spouse = applyDraftToPlan(draft, p).members.find(m => m.id === spouseId);

        expect(spouse).toMatchObject({ name: "Spouse", currentAge: 43, currentIncome: 90_000, retirementAge: 67, growthStrategy: "Growth" });
    });

    it("round-trips an untouched draft to the same plan", () => {
        const p = plan();
        const updated = applyDraftToPlan(draftFromPlan(p), p);

        expect(updated.expectedReturnRate).toBe(p.expectedReturnRate);
        expect(updated.members.map(m => m.retirementAge)).toEqual([65, 67]);
    });
});
