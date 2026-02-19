import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getAllForecastPlansOptions,
    getAllForecastPlansQueryKey,
    getForecastPlanOptions,
    getForecastPlanQueryKey,
    runForecastMutation,
    createForecastPlanMutation,
    updateForecastPlanMutation,
    deleteForecastPlanMutation,
    createPlannedItemMutation,
    updatePlannedItemMutation,
    deletePlannedItemMutation,
} from "api/@tanstack/react-query.gen";
import type { ForecastPlan, ForecastResult, PlannedItem } from "api/types.gen";

export const forecastKey = ["forecast"];

export const useForecastPlans = (includeArchived: boolean = false) =>
    useQuery({
        ...getAllForecastPlansOptions({ query: { IncludeArchived: includeArchived } }),
    });

export const useForecastPlan = (planId: string) =>
    useQuery({
        ...getForecastPlanOptions({ path: { id: planId } }),
        enabled: !!planId,
    });

export const useRunForecast = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, data, isPending } = useMutation({
        ...runForecastMutation(),
        onSuccess: (_data, variables) => {
            queryClient.setQueryData(
                getForecastPlanQueryKey({ path: { id: variables.path!.planId } }),
                undefined,
            );
            queryClient.setQueryData(
                [...forecastKey, variables.path!.planId, "result"],
                _data,
            );
        },
    });

    const run = (planId: string) => {
        mutate({ path: { planId } });
    };

    const runAsync = (planId: string) => {
        return mutateAsync({ path: { planId } });
    };

    return { run, runAsync, result: data as ForecastResult | undefined, isPending };
};

export const useCreateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, isPending } = useMutation({
        ...createForecastPlanMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
        },
    });

    const create = (plan: Partial<ForecastPlan>) => {
        mutate({ body: plan as any });
    };

    const createAsync = (plan: Partial<ForecastPlan>) => {
        return mutateAsync({ body: plan as any });
    };

    return { create, createAsync, isPending };
};

export const useUpdateForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useMutation({
        ...updateForecastPlanMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
            queryClient.invalidateQueries({ queryKey: [...forecastKey, variables.path!.id, "result"] });
        },
    });

    const update = (planId: string, plan: Partial<ForecastPlan>) => {
        mutate({ body: plan as any, path: { id: planId } });
    };

    return { update, isPending };
};

export const useDeleteForecastPlan = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...deleteForecastPlanMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAllForecastPlansQueryKey() });
        },
    });

    const deletePlan = (planId: string) => {
        mutate({ path: { id: planId } });
    };

    return deletePlan;
};

export const useCreatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useMutation({
        ...createPlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: [...forecastKey, variables.path!.planId, "result"] });
        },
    });

    const create = (planId: string, item: Partial<PlannedItem>) => {
        mutate({ body: item as any, path: { planId } });
    };

    return { create, isPending };
};

export const useUpdatePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useMutation({
        ...updatePlannedItemMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: [...forecastKey, variables.path!.planId, "result"] });
        },
    });

    const update = (planId: string, itemId: string, item: Partial<PlannedItem>) => {
        mutate({ body: item as any, path: { planId, itemId } });
    };

    return { update, isPending };
};

export const useDeletePlannedItem = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...deletePlannedItemMutation(),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: getForecastPlanQueryKey({ path: { id: variables.path!.planId } }) });
            queryClient.invalidateQueries({ queryKey: [...forecastKey, variables.path!.planId, "result"] });
        },
    });

    const deleteItem = (planId: string, itemId: string) => {
        mutate({ path: { planId, itemId } });
    };

    return deleteItem;
};
