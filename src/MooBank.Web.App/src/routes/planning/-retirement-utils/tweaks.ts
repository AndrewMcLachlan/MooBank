import type { RetirementMemberOverride, RetirementPlan, RetirementProjectionOverrides, SimpleRetirementPlan } from "api/types.gen";

/**
 * The tweak sliders' working copy. It is a full set of overrides seeded from the saved plan, so
 * every slider has a position and an untouched draft projects exactly as the plan does.
 *
 * It is only ever sent as overrides, never saved, until the user chooses to lock it in.
 */
export const draftFromPlan = (plan: RetirementPlan): RetirementProjectionOverrides => ({
    expectedReturnRate: plan.expectedReturnRate,
    inflationRate: plan.inflationRate,
    superGuaranteeRate: plan.superGuaranteeRate,
    contributionsTaxRate: plan.contributionsTaxRate,
    lifeExpectancy: plan.lifeExpectancy,
    members: plan.members.map(m => ({
        memberId: m.id,
        currentAge: m.currentAge,
        currentIncome: m.currentIncome,
        salarySacrifice: m.salarySacrifice,
        retirementAge: m.retirementAge,
        growthStrategy: m.growthStrategy,
    })),
});

/** Whether the draft still matches the saved plan. */
export const isDirty = (draft: RetirementProjectionOverrides, plan: RetirementPlan) =>
    JSON.stringify(draft) !== JSON.stringify(draftFromPlan(plan));

/** Replace one member's values within the draft, leaving the others alone. */
export const withMember = (
    draft: RetirementProjectionOverrides,
    memberId: string,
    changes: Partial<RetirementMemberOverride>,
): RetirementProjectionOverrides => ({
    ...draft,
    members: draft.members.map(m => m.memberId === memberId ? { ...m, ...changes } : m),
});

/**
 * Fold the draft back onto the plan, ready to save.
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
    members: plan.members.map(member => {
        const tweak = draft.members.find(m => m.memberId === member.id);

        return {
            ...member,
            currentAge: tweak?.currentAge ?? member.currentAge,
            currentIncome: tweak?.currentIncome ?? member.currentIncome,
            salarySacrifice: tweak?.salarySacrifice ?? member.salarySacrifice,
            retirementAge: tweak?.retirementAge ?? member.retirementAge,
            growthStrategy: tweak?.growthStrategy ?? member.growthStrategy,
        };
    }),
});
