import type { RetirementPlanMember, SimpleRetirementPlan } from "api/types.gen";

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

export const emptyMember = (name: string): RetirementPlanMember => ({
    name,
    dateOfBirth: "",
    currentIncome: 0,
    retirementAge: defaultRetirementAge,
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
