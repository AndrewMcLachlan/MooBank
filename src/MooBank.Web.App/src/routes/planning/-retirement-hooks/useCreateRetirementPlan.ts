import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createRetirementPlanMutation, getAllRetirementPlansQueryKey } from "api/@tanstack/react-query.gen";
import type { SimpleRetirementPlan } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateRetirementPlan = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, isPending } = useMutation({
        ...createRetirementPlanMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllRetirementPlansQueryKey() });
        },
    });

    const createAsync = (plan: SimpleRetirementPlan) =>
        toast.promise(mutateAsync({ body: plan }), { pending: "Creating retirement plan", success: "Retirement plan created", error: "Failed to create retirement plan" });

    return { createAsync, isPending };
};
