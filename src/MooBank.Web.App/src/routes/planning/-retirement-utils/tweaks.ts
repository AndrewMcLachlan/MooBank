import type { RetirementMemberOverride, RetirementPlan, RetirementProjectionOverrides, SimpleRetirementPlan } from "api/types.gen";

/**
 * The tweak sliders' working copy: only the values someone has actually moved.
 *
 * Deliberately sparse rather than a snapshot of the whole plan. A slider nobody has touched is
 * simply absent, so it reads through to the plan — which means editing the plan's settings moves
 * every untouched slider to the new value while the tweaks in progress survive. A full snapshot
 * would pin every slider to whatever the plan held when the first one moved.
 *
 * Nothing here is saved until it is locked in; see the overrides model on the server.
 */
export const emptyDraft: RetirementProjectionOverrides = { members: [], excludedMemberIds: [] };

/** Whether a member is currently left out of the projection. */
export const isExcluded = (draft: RetirementProjectionOverrides, memberId: string) =>
    (draft.excludedMemberIds ?? []).includes(memberId);

/**
 * Leave a member out of the projection, or put them back.
 *
 * A view of the plan rather than an edit to it, so it never forms part of what "lock in" saves —
 * seeing the plan without someone must not be a step towards removing them from it.
 */
export const withExcluded = (draft: RetirementProjectionOverrides, memberId: string, excluded: boolean): RetirementProjectionOverrides => {
    const current = draft.excludedMemberIds ?? [];

    return {
        ...draft,
        excludedMemberIds: excluded ? [...new Set([...current, memberId])] : current.filter(id => id !== memberId),
    };
};

/**
 * The plan-level values a tweak can override.
 *
 * Named once and derived from, because the same list is needed both to set a value and to decide
 * whether anything has been tweaked. Written out by hand in both places, adding a value to one and
 * not the other left it silently unable to mark the draft dirty.
 */
export const planTweakKeys = [
    "expectedReturnRate",
    "inflationRate",
    "superGuaranteeRate",
    "contributionsTaxRate",
    "lifeExpectancy",
    "targetRetirementIncome",
    "preRetirementSwitchYears",
    "cashReturnRate",
] as const;

export type PlanTweakKey = typeof planTweakKeys[number];

/** The value a slider should show: the tweak if there is one, otherwise the plan's own value. */
export const planValue = <K extends keyof RetirementPlan & keyof RetirementProjectionOverrides>(
    draft: RetirementProjectionOverrides,
    plan: RetirementPlan,
    key: K,
) => (draft[key] ?? plan[key]) as RetirementPlan[K];

/** The value a member's slider should show. */
export const memberValue = <K extends keyof RetirementMemberOverride>(
    draft: RetirementProjectionOverrides,
    plan: RetirementPlan,
    memberId: string,
    key: K,
) => {
    const tweak = draft.members.find(m => m.memberId === memberId)?.[key];
    if (tweak !== undefined && tweak !== null) return tweak;

    const member = plan.members.find(m => m.id === memberId);
    return member?.[key as keyof typeof member] as RetirementMemberOverride[K];
};

/**
 * Whether anything has been tweaked.
 *
 * Overrides naming a member the plan no longer has are ignored, so removing someone in settings
 * cannot leave the page claiming a what-if that has nothing behind it.
 */
export const isDirty = (draft: RetirementProjectionOverrides, plan: RetirementPlan) => {
    const planLevel = planTweakKeys.some(k => draft[k] !== undefined && draft[k] !== null);

    // An exclusion only counts while the member is still on the plan, for the same reason a member
    // override does: removing someone in settings must not leave the page claiming a what-if.
    const excluded = (draft.excludedMemberIds ?? []).some(id => plan.members.some(m => m.id === id));

    const memberLevel = draft.members.some(m =>
        plan.members.some(p => p.id === m.memberId) &&
        (["currentAge", "currentIncome", "salarySacrifice", "retirementAge", "growthStrategy", "annualFees", "insurancePremium"] as const)
            .some(k => m[k] !== undefined && m[k] !== null));

    return planLevel || memberLevel || excluded;
};

/** Set one plan-level value, or clear it when it matches the plan again. */
export const withPlanValue = <K extends PlanTweakKey>(
    draft: RetirementProjectionOverrides,
    plan: RetirementPlan,
    key: K,
    value: RetirementProjectionOverrides[K],
): RetirementProjectionOverrides => {
    const next = { ...draft };

    // Moving a slider back to the plan's own value is the same as never having moved it, so the
    // override is dropped rather than left sitting there equal to the plan.
    if (value === plan[key]) delete next[key];
    else next[key] = value;

    return next;
};

/** Set one of a member's values, or clear it when it matches the plan again. */
export const withMemberValue = <K extends "currentAge" | "currentIncome" | "salarySacrifice" | "retirementAge" | "growthStrategy" | "annualFees" | "insurancePremium">(
    draft: RetirementProjectionOverrides,
    plan: RetirementPlan,
    memberId: string,
    key: K,
    value: RetirementMemberOverride[K],
): RetirementProjectionOverrides => {
    const member = plan.members.find(m => m.id === memberId);
    const existing = draft.members.find(m => m.memberId === memberId);

    const updated: RetirementMemberOverride = { ...(existing ?? { memberId }), [key]: value };
    if (member && value === member[key as keyof typeof member]) delete updated[key];

    const stillTweaked = (["currentAge", "currentIncome", "salarySacrifice", "retirementAge", "growthStrategy", "annualFees", "insurancePremium"] as const)
        .some(k => updated[k] !== undefined && updated[k] !== null);

    return {
        ...draft,
        members: stillTweaked
            ? [...draft.members.filter(m => m.memberId !== memberId), updated]
            : draft.members.filter(m => m.memberId !== memberId),
    };
};

/**
 * Fold the draft onto the plan, ready to save.
 *
 * Members are matched by id and keep everything the sliders do not touch — their name and their
 * superannuation accounts.
 */
export const applyDraftToPlan = (draft: RetirementProjectionOverrides, plan: RetirementPlan): SimpleRetirementPlan => ({
    name: plan.name,
    expectedReturnRate: draft.expectedReturnRate ?? plan.expectedReturnRate,
    inflationRate: draft.inflationRate ?? plan.inflationRate,
    superGuaranteeRate: draft.superGuaranteeRate ?? plan.superGuaranteeRate,
    contributionsTaxRate: draft.contributionsTaxRate ?? plan.contributionsTaxRate,
    lifeExpectancy: draft.lifeExpectancy ?? plan.lifeExpectancy,
    targetRetirementIncome: draft.targetRetirementIncome ?? plan.targetRetirementIncome,
    preRetirementSwitchYears: draft.preRetirementSwitchYears ?? plan.preRetirementSwitchYears,
    cashReturnRate: draft.cashReturnRate ?? plan.cashReturnRate,
    members: plan.members.map(member => {
        const tweak = draft.members.find(m => m.memberId === member.id);

        return {
            ...member,
            currentAge: tweak?.currentAge ?? member.currentAge,
            currentIncome: tweak?.currentIncome ?? member.currentIncome,
            salarySacrifice: tweak?.salarySacrifice ?? member.salarySacrifice,
            retirementAge: tweak?.retirementAge ?? member.retirementAge,
            growthStrategy: tweak?.growthStrategy ?? member.growthStrategy,
            annualFees: tweak?.annualFees ?? member.annualFees,
            insurancePremium: tweak?.insurancePremium ?? member.insurancePremium,
        };
    }),
});

/**
 * Drop overrides for members the plan no longer has, so a projection is never run against a
 * member who was removed in settings.
 */
export const pruneDraft = (draft: RetirementProjectionOverrides, plan: RetirementPlan): RetirementProjectionOverrides => ({
    ...draft,
    members: draft.members.filter(m => plan.members.some(p => p.id === m.memberId)),
});
