import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateFamilyMutation, getAllFamiliesQueryKey, getFamilyQueryKey } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "react-toastify";

export const useUpdateFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateFamilyMutation(),
        onMutate: (variables) => {
            const data = variables.body as Family;
            let allFamilies = queryClient.getQueryData<Family[]>(getAllFamiliesQueryKey());
            if (!allFamilies) {
                console.warn("Query Cache is missing Families");
                return;
            }

            allFamilies = allFamilies.map((family) => family.id === data.id ? data : family);
            allFamilies = allFamilies.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Family[]>(getAllFamiliesQueryKey(), allFamilies);

            queryClient.setQueryData<Family>(getFamilyQueryKey({ path: { id: data.id } }), data);
        },
        onError: (_error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllFamiliesQueryKey() });
            queryClient.invalidateQueries({ queryKey: getFamilyQueryKey({ path: { id: variables.path!.id } }) });
        }
    });

    return (family: Family) =>
        toast.promise(mutateAsync({ body: family, path: { id: family.id } }), { pending: "Updating family", success: "Family updated", error: "Failed to update family" });
}
