export const forecastKey = ["forecast"];

export const forecastResultQueryKey = (planId: string) => [...forecastKey, planId, "result"];
