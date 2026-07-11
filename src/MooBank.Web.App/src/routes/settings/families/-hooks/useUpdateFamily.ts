import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateFamilyMutation, getAllFamiliesQueryKey, getFamilyQueryKey } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateFamilyMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllFamiliesQueryKey() });
            queryClient.invalidateQueries({ queryKey: getFamilyQueryKey({ path: { id: variables.path!.id } }) });
        }
    });

    return (family: Family) =>
        toast.promise(mutateAsync({ body: family, path: { id: family.id } }), { pending: "Updating family", success: "Family updated", error: "Failed to update family" });
}
