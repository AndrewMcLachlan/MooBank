/**
 * The link between a target retirement income and how long the savings have to last.
 *
 * For a given balance and return, these two are the same equation read from either end: name the
 * horizon and the sustainable income follows, name the income and the horizon follows. Keeping them
 * solved from each other means the two sliders can never quietly disagree.
 *
 * The arithmetic is the standard annuity, matching the server's own sustainable-income figure — the
 * same model read forwards and backwards, so the two directions always agree with each other. It
 * deliberately leaves the Age Pension out, exactly as that figure does. The pension only ever adds
 * income, so a plan solved here lasts a little longer than the sliders promise rather than falling
 * short; the projection's own "money runs out" is what reports the truth.
 */

/** Below this, a rate is treated as zero to avoid dividing by something indistinguishable from it. */
const negligible = 0.000001;

/**
 * The level annual income a balance supports over a number of years.
 *
 * The present-value-of-an-annuity formula rearranged for the payment. At a negligible real return it
 * degenerates to dividing the balance evenly, which is also the right answer in the limit.
 */
export const sustainableIncome = (balance: number, realReturnRate: number, years: number): number => {
    if (years <= 0 || balance <= 0) return 0;

    if (Math.abs(realReturnRate) < negligible) return balance / years;

    const discountFactor = 1 - Math.pow(1 + realReturnRate, -years);

    // A real return at or below -100% leaves nothing to draw on.
    if (discountFactor <= 0) return 0;

    return (balance * realReturnRate) / discountFactor;
};

/**
 * How many years a balance supports a given level annual income.
 *
 * The inverse of {@link sustainableIncome}. An income at or below what the return alone provides
 * never exhausts the balance, so it returns null — there is no finite answer to sync a slider to.
 */
export const yearsThatLast = (balance: number, realReturnRate: number, income: number): number | null => {
    if (income <= 0 || balance <= 0) return null;

    if (Math.abs(realReturnRate) < negligible) return balance / income;

    // Drawing no more than the real return leaves the balance intact for ever.
    const ratio = (balance * realReturnRate) / income;
    if (ratio >= 1) return null;

    return -Math.log(1 - ratio) / Math.log(1 + realReturnRate);
};

/**
 * What a plan's figures imply for the pairing, given a projection.
 *
 * Everything needed is already on the summary, so the sliders can solve without another round trip.
 */
export interface SyncBasis {
    /** The household balance at retirement, in the same dollars a target income is stated in. */
    balance: number;
    realReturnRate: number;
    /** The age the last member retires, which is where the drawdown horizon starts. */
    retirementAge: number;
}

/** The income that exactly exhausts the balance by the given age. */
export const incomeForAge = (basis: SyncBasis, age: number): number =>
    Math.round(sustainableIncome(basis.balance, basis.realReturnRate, age - basis.retirementAge) / 1000) * 1000;

/** The oldest age a plan can be set to last until, matching the server's own limit. */
export const maxPlanAge = 120;

/**
 * The age the balance is exhausted at, drawing the given income.
 *
 * Null when there is no age worth syncing to: either the income is small enough that the balance
 * never runs down, or it lasts past any age a plan can be set to. Returning a clamped age in those
 * cases would be worse than returning nothing — the two controls are linked precisely so that
 * neither can state something the other contradicts, and a clamped age states a year the money does
 * not actually run out in.
 */
export const ageForIncome = (basis: SyncBasis, income: number): number | null => {
    const years = yearsThatLast(basis.balance, basis.realReturnRate, income);

    if (years === null) return null;

    const age = Math.round(basis.retirementAge + years);

    return age > maxPlanAge ? null : age;
};

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
