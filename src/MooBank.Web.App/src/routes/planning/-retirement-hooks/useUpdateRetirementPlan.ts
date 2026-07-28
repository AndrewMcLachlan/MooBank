import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getAllRetirementPlansQueryKey, getRetirementPlanQueryKey, updateRetirementPlanMutation } from "api/@tanstack/react-query.gen";
import type { SimpleRetirementPlan } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { retirementProjectionQueryKey } from "./keys";

export const useUpdateRetirementPlan = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...updateRetirementPlanMutation(),
        onSettled: (_data, _error, variables) => {
            const planId = variables.path!.id;
            queryClient.invalidateQueries({ queryKey: getAllRetirementPlansQueryKey() });
            queryClient.invalidateQueries({ queryKey: getRetirementPlanQueryKey({ path: { id: planId } }) });
            // The assumptions drive the projection, so any change to them makes it stale.
            queryClient.invalidateQueries({ queryKey: retirementProjectionQueryKey(planId) });
        },
    });

    const updateAsync = (planId: string, plan: SimpleRetirementPlan) =>
        toast.promise(mutateAsync({ body: plan, path: { id: planId } }), { pending: "Updating retirement plan", success: "Retirement plan updated", error: "Failed to update retirement plan" });

    return { updateAsync, isPending };
};
