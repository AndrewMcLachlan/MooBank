import { describe, it, expect } from "vitest";
import { householdKey, includedMemberIds, projectionMatchesDraft } from "./retirementSync";





/**
 * The controls are linked so that neither can state something the other contradicts. That makes
 * "no answer" the right result whenever an age would have to be clamped to be representable — a
 * clamped age names a year the money does not actually run out in.
 */

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
