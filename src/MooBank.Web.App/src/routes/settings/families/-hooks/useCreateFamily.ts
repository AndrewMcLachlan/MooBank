import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createFamilyMutation, getAllFamiliesQueryKey } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...createFamilyMutation(),
        onMutate: async (variables) => {
            const queryKey = getAllFamiliesQueryKey();
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<Family[]>(queryKey);
            if (previous) {
                const next = [...previous, variables.body as Family].sort((f1, f2) => f1.name.localeCompare(f2.name));
                queryClient.setQueryData<Family[]>(queryKey, next);
            }

            return { previous };
        },
        onError: (_error, _variables, context: any) => {
            if (context?.previous) {
                queryClient.setQueryData(getAllFamiliesQueryKey(), context.previous);
            }
        },
        onSettled: () => {
            // Refetch so the optimistic entry (which has no server-assigned id) is replaced.
            queryClient.invalidateQueries({ queryKey: getAllFamiliesQueryKey() });
        },
    });

    return (family: Family) =>
        toast.promise(mutateAsync({ body: family }), { pending: "Creating family", success: "Family created", error: "Failed to create family" });
}
