import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getAllFamiliesOptions, getAllFamiliesQueryKey, getFamilyOptions, getFamilyQueryKey, createFamilyMutation, updateFamilyMutation } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "react-toastify";

export const useFamilies = () => useQuery({ ...getAllFamiliesOptions(), staleTime: 1000 * 60 * 5 });

export const useFamily = (id: string) => useQuery({ ...getFamilyOptions({ path: { id } }), enabled: !!id });

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
