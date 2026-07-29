import type { GrowthStrategy, RetirementPlanMember, SimpleRetirementPlan } from "api/types.gen";

/**
 * Starting assumptions for a new plan, matching the Australian settings a superannuation
 * calculator would default to. Every one of them is editable; they exist so a new plan produces a
 * meaningful projection before anything is tuned.
 */
export const defaultAssumptions = {
    expectedReturnRate: 0.065,
    inflationRate: 0.025,
    superGuaranteeRate: 0.12,
    contributionsTaxRate: 0.15,
    lifeExpectancy: 90,
    /**
     * Left at nought deliberately: what a household means to live on is theirs to say, and a made-up
     * figure here would produce a "your money runs out" verdict nobody asked for.
     */
    targetRetirementIncome: 0,
    /** The de-risking glide most funds apply, moving a balance to cash as it nears being drawn on. */
    preRetirementSwitchYears: 2,
    cashReturnRate: 0.03,
} as const;

export const defaultRetirementAge = 67;

export const defaultCurrentAge = 40;

/**
 * The youngest age a plan member can be given. Superannuation guarantee contributions follow
 * employment, so anyone in a projection is of working age; the floor exists to stop an unset age
 * reading as a newborn and producing a projection running most of a century.
 */
export const minWorkingAge = 15;

/**
 * The investment options a member can be projected under. The rates each one implies live on the
 * server; the projection reports the rate it used, so nothing here needs to restate them.
 */
export const growthStrategies: { value: GrowthStrategy; label: string }[] = [
    { value: "Conservative", label: "Conservative" },
    { value: "Balanced", label: "Balanced" },
    { value: "Growth", label: "Growth" },
    { value: "HighGrowth", label: "High Growth" },
    { value: "Custom", label: "Custom (use the plan's rate)" },
];

export const emptyMember = (): RetirementPlanMember => ({
    userId: "",
    currentAge: defaultCurrentAge,
    currentIncome: 0,
    salarySacrifice: 0,
    retirementAge: defaultRetirementAge,
    growthStrategy: "Balanced",
    annualFees: 0,
    insurancePremium: 0,
    instrumentIds: [],
});

export const emptyPlan = (): SimpleRetirementPlan => ({
    name: "Retirement",
    ...defaultAssumptions,
    members: [],
});

/** Rates are held as fractions but shown as percentages. */
export const toPercent = (rate: number) => Math.round((rate ?? 0) * 1000) / 10;

export const fromPercent = (percent: number) => (Number(percent) || 0) / 100;
