import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteRetirementPlanMutation, getAllRetirementPlansQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useDeleteRetirementPlan = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...deleteRetirementPlanMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllRetirementPlansQueryKey() });
        },
    });

    return (planId: string) =>
        toast.promise(mutateAsync({ path: { id: planId } }), { pending: "Deleting retirement plan", success: "Retirement plan deleted", error: "Failed to delete retirement plan" });
};
