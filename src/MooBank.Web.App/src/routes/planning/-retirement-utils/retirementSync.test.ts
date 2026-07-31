import { describe, it, expect } from "vitest";
import { ageForIncome, householdKey, incomeForAge, includedMemberIds, projectionMatchesDraft, sustainableIncome, yearsThatLast, type SyncBasis } from "./retirementSync";

const basis: SyncBasis = { balance: 1_000_000, realReturnRate: 0.04, retirementAge: 67 };

describe("sustainableIncome", () => {
    it("spends a balance evenly when the real return is nought", () => {
        expect(sustainableIncome(1_000_000, 0, 20)).toBe(50_000);
    });

    it("supports more than an even split when the balance earns a return", () => {
        expect(sustainableIncome(1_000_000, 0.04, 20)).toBeGreaterThan(50_000);
    });

    it("supports less over a longer horizon", () => {
        expect(sustainableIncome(1_000_000, 0.04, 30)).toBeLessThan(sustainableIncome(1_000_000, 0.04, 20));
    });

    it("has nothing to support with no balance or no years", () => {
        expect(sustainableIncome(0, 0.04, 20)).toBe(0);
        expect(sustainableIncome(1_000_000, 0.04, 0)).toBe(0);
    });
});

describe("yearsThatLast", () => {
    it("is the inverse of sustainableIncome", () => {
        const income = sustainableIncome(1_000_000, 0.04, 25);

        expect(yearsThatLast(1_000_000, 0.04, income)).toBeCloseTo(25, 6);
    });

    it("divides evenly when the real return is nought", () => {
        expect(yearsThatLast(1_000_000, 0, 50_000)).toBe(20);
    });

    /**
     * Drawing no more than the real return never touches the capital, so there is no year it runs
     * out and nothing to sync a slider to.
     */
    it("never runs out when the income is within the return", () => {
        expect(yearsThatLast(1_000_000, 0.04, 40_000)).toBeNull();
        expect(yearsThatLast(1_000_000, 0.04, 30_000)).toBeNull();
    });

    it("has no answer without an income or a balance", () => {
        expect(yearsThatLast(1_000_000, 0.04, 0)).toBeNull();
        expect(yearsThatLast(0, 0.04, 50_000)).toBeNull();
    });
});

describe("the two sliders staying in sync", () => {
    /** The promise: solving either way lands back where it started. */
    it("round-trips an age through its income", () => {
        const income = incomeForAge(basis, 90);

        expect(ageForIncome(basis, income)).toBe(90);
    });

    it("round-trips an income through its age", () => {
        const age = ageForIncome(basis, 70_000);

        expect(age).not.toBeNull();
        // Rounded to the nearest thousand, so the income comes back within that.
        expect(Math.abs(incomeForAge(basis, age!) - 70_000)).toBeLessThanOrEqual(1_000);
    });

    it("gives a higher income for a shorter retirement", () => {
        expect(incomeForAge(basis, 85)).toBeGreaterThan(incomeForAge(basis, 95));
    });

    it("brings the age back when the income is raised", () => {
        const modest = ageForIncome(basis, 60_000);
        const generous = ageForIncome(basis, 120_000);

        expect(generous).not.toBeNull();
        expect(generous!).toBeLessThan(modest!);
    });

    it("rounds the income to a round figure a slider can land on", () => {
        expect(incomeForAge(basis, 90) % 1_000).toBe(0);
    });

    it("has no age to offer for an income the balance never exhausts", () => {
        expect(ageForIncome(basis, 20_000)).toBeNull();
    });

    it("offers nothing for a plan with no balance yet", () => {
        const empty: SyncBasis = { balance: 0, realReturnRate: 0.04, retirementAge: 67 };

        expect(incomeForAge(empty, 90)).toBe(0);
        expect(ageForIncome(empty, 50_000)).toBeNull();
    });
});

/**
 * The controls are linked so that neither can state something the other contradicts. That makes
 * "no answer" the right result whenever an age would have to be clamped to be representable — a
 * clamped age names a year the money does not actually run out in.
 */
describe("refusing to state an age it cannot honour", () => {
    it("offers no age when the money lasts past any age a plan can hold", () => {
        // Just above the real return, so it runs down — but over centuries.
        const income = incomeForAge(basis, 400);

        expect(income).toBeGreaterThan(0);
        expect(ageForIncome(basis, income)).toBeNull();
    });

    it("offers the true age for a very high income rather than a floor", () => {
        // Half the balance a year exhausts it in about two, well below any slider minimum.
        const age = ageForIncome(basis, 500_000);

        expect(age).not.toBeNull();
        expect(age!).toBeLessThan(70);
        expect(age!).toBeGreaterThan(basis.retirementAge);
    });
});

/**
 * The guard that decides when it is safe to re-solve the target income after someone is included or
 * excluded.
 *
 * An earlier version keyed on the draft alone and read the summary while the projection on screen
 * still belonged to the previous household. Both failures it produced are pinned below, because
 * neither is visible from the code — one changes nothing at all, and the other changes the wrong
 * number convincingly.
 */
describe("matching a projection to the household a draft asks for", () => {
    const self = "self";
    const spouse = "spouse";
    const planMembers = [self, spouse];

    it("matches when nobody is excluded and both are projected", () => {
        expect(projectionMatchesDraft(planMembers, [], [self, spouse])).toBe(true);
        expect(projectionMatchesDraft(planMembers, undefined, [spouse, self])).toBe(true);
    });

    it("matches when one is excluded and only the other is projected", () => {
        expect(projectionMatchesDraft(planMembers, [spouse], [self])).toBe(true);
    });

    /**
     * The moment the old guard got wrong: the exclusion is in the draft, but the projection still
     * describes both people. Re-solving here uses the household being left behind, so the income
     * does not appear to change at all.
     */
    it("does not match a projection still showing the person just excluded", () => {
        expect(projectionMatchesDraft(planMembers, [spouse], [self, spouse])).toBe(false);
    });

    /**
     * And the mirror image: the person is back in the draft, but the projection on screen is still
     * the one-person run. Re-solving here produces the single-person income for a couple.
     */
    it("does not match a projection still showing only one when both are wanted", () => {
        expect(projectionMatchesDraft(planMembers, [], [self])).toBe(false);
    });

    it("treats a household as the same however its members are ordered", () => {
        expect(householdKey([spouse, self])).toBe(householdKey([self, spouse]));
    });

    it("works out who a draft is asking for", () => {
        expect(includedMemberIds(planMembers, [spouse])).toEqual([self]);
        expect(includedMemberIds(planMembers, [])).toEqual(planMembers);
        expect(includedMemberIds(planMembers, undefined)).toEqual(planMembers);
        // An id that is not on the plan excludes nobody.
        expect(includedMemberIds(planMembers, ["someone-else"])).toEqual(planMembers);
    });
});
