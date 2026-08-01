import type { RetirementProjectionOverrides } from "api/types.gen";

export const retirementKey = ["retirement"];

/**
 * The key for a plan's projection. Overrides form part of it, so each set of slider positions
 * caches its own result; an unmodified run keys on "saved" so it stays distinct from a tweak that
 * happens to match the plan.
 *
 * Passing only a plan id gives the prefix that invalidates every projection for that plan,
 * tweaked or not.
 */
export const retirementProjectionQueryKey = (planId: string, overrides?: RetirementProjectionOverrides) =>
    overrides === undefined
        ? [...retirementKey, planId, "projection"]
        : [...retirementKey, planId, "projection", overrides ?? "saved"];
