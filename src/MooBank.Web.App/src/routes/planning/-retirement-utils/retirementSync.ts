/**
 * Working out which projection on screen belongs to which draft.
 *
 * Deliberately holds no financial arithmetic. Every figure the retirement planner shows is solved by
 * the API against the projection itself — the sustainable income, the pension, the year the money
 * runs short — because an approximation of any of them here would disagree with the projection
 * beside it. What is left is bookkeeping about which result answers which question.
 */

/**
 * The household a set of member ids describes, as a comparable key.
 *
 * Order is not meaningful, so it is normalised away — two lists of the same people are the same
 * household however they arrived.
 */
export const householdKey = (memberIds: readonly string[]) => [...memberIds].sort().join(",");

/**
 * The people a draft is asking to see, given the plan it applies to.
 */
export const includedMemberIds = (
    planMemberIds: readonly string[],
    excludedMemberIds: readonly string[] | undefined,
) => planMemberIds.filter(id => !(excludedMemberIds ?? []).includes(id));

/**
 * Whether a projection describes the household a draft is asking for.
 *
 * Compares who is actually in the result against who the draft wants, rather than trusting that a
 * request has caught up. A draft reaches the query debounced, so for a moment the projection on
 * screen belongs to the previous set of people — and anything that reads the summary in that moment
 * is reading the wrong household. Asking the result who is in it cannot drift out of step the way a
 * timing guard can.
 */
export const projectionMatchesDraft = (
    planMemberIds: readonly string[],
    excludedMemberIds: readonly string[] | undefined,
    projectedMemberIds: readonly string[],
) => householdKey(includedMemberIds(planMemberIds, excludedMemberIds)) === householdKey(projectedMemberIds);
