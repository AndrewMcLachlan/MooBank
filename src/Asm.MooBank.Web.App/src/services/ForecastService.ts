import { useQueryClient } from "@tanstack/react-query";
import { useApiDelete, useApiGet, useApiPatch, useApiPost, useApiPut } from "@andrewmclachlan/moo-app";
import { CreateForecastPlan, CreatePlannedItem, ForecastPlan, ForecastResult, PlannedItem } from "../models";

interface PlanVariables {
    planId: string;
}

interface PlannedItemVariables {
    planId: string;
    itemId?: string;
}

export const forecastKey = ["forecast"];

export const useForecastPlans = (includeArchived: boolean = false) =>
    useApiGet<ForecastPlan[]>([forecastKey, "list", includeArchived], `api/forecast/plans?includeArchived=${includeArchived}`);

export const useForecastPlan = (planId: string) =>
    useApiGet<ForecastPlan>([forecastKey, planId], `api/forecast/plans/${planId}`, {
        enabled: !!planId
    });

export const useRunForecast = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, data, isPending } = useApiPost<ForecastResult, PlanVariables, null>(
        (variables) => `api/forecast/plans/${variables.planId}/run`,
        {
            onSuccess: (_data, [variables]) => {
                queryClient.setQueryData([forecastKey, variables.planId, "result"], _data);
            }
        }
    );

    const run = (planId: string) => {
        mutate([{ planId }, null]);
    };

    const runAsync = (planId: string) => {
        return mutateAsync([{ planId }, null]);
    };

    return { run, runAsync, result: data, isPending };
};

export const useCreateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, isPending } = useApiPost<ForecastPlan, null, CreateForecastPlan>(
        () => `api/forecast/plans`,
        {
            onSettled: () => {
                queryClient.invalidateQueries({ queryKey: [forecastKey] });
            }
        }
    );

    const create = (plan: CreateForecastPlan) => {
        mutate([null, plan]);
    };

    const createAsync = (plan: CreateForecastPlan) => {
        return mutateAsync([null, plan]);
    };

    return { create, createAsync, isPending };
};

export const useUpdateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useApiPut<ForecastPlan, PlanVariables, Partial<ForecastPlan>>(
        (variables) => `api/forecast/plans/${variables.planId}`,
        {
            onSettled: (_data, _error, [variables]) => {
                queryClient.invalidateQueries({ queryKey: [forecastKey] });
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId, "result"] });
            }
        }
    );

    const update = (planId: string, plan: Partial<ForecastPlan>) => {
        mutate([{ planId }, plan]);
    };

    return { update, isPending };
};

export const useDeleteForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiDelete<PlanVariables>(
        (variables) => `api/forecast/plans/${variables.planId}`,
        {
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: [forecastKey] });
            }
        }
    );

    const deletePlan = (planId: string) => {
        mutate({ planId });
    };

    return deletePlan;
};

export const useCreatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useApiPost<PlannedItem, PlannedItemVariables, CreatePlannedItem>(
        (variables) => `api/forecast/plans/${variables.planId}/items`,
        {
            onSettled: (_data, _error, [variables]) => {
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId] });
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId, "result"] });
            }
        }
    );

    const create = (planId: string, item: CreatePlannedItem) => {
        mutate([{ planId }, item]);
    };

    return { create, isPending };
};

export const useUpdatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useApiPut<PlannedItem, PlannedItemVariables, Partial<PlannedItem>>(
        (variables) => `api/forecast/plans/${variables.planId}/items/${variables.itemId}`,
        {
            onSettled: (_data, _error, [variables]) => {
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId] });
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId, "result"] });
            }
        }
    );

    const update = (planId: string, itemId: string, item: Partial<PlannedItem>) => {
        mutate([{ planId, itemId }, item]);
    };

    return { update, isPending };
};

export const useDeletePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiDelete<PlannedItemVariables>(
        (variables) => `api/forecast/plans/${variables.planId}/items/${variables.itemId}`,
        {
            onSuccess: (_data, variables) => {
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId] });
                queryClient.invalidateQueries({ queryKey: [forecastKey, variables.planId, "result"] });
            }
        }
    );

    const deleteItem = (planId: string, itemId: string) => {
        mutate({ planId, itemId });
    };

    return deleteItem;
};
