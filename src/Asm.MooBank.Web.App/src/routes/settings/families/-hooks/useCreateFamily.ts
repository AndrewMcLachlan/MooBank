import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createFamilyMutation, getAllFamiliesQueryKey } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "react-toastify";

export const useCreateFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...createFamilyMutation(),
        onMutate: (variables) => {
            let allFamilies = queryClient.getQueryData<Family[]>(getAllFamiliesQueryKey());
            if (!allFamilies) {
                console.warn("Query Cache is missing Families");
                return;
            }

            allFamilies.push(variables.body as Family);
            allFamilies = allFamilies.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Family[]>(getAllFamiliesQueryKey(), allFamilies);
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: getAllFamiliesQueryKey() });
        }
    });

    return (family: Family) =>
        toast.promise(mutateAsync({ body: family }), { pending: "Creating family", success: "Family created", error: "Failed to create family" });
}
