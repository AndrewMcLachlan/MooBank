export const retirementKey = ["retirement"];

export const retirementProjectionQueryKey = (planId: string) => [...retirementKey, planId, "projection"];
