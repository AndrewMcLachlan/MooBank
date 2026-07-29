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
} as const;

export const defaultRetirementAge = 67;

export const defaultCurrentAge = 40;

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

export const emptyMember = (name: string): RetirementPlanMember => ({
    name,
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
