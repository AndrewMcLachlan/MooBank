import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";

import { Family } from "models";
import { toast } from "react-toastify";

const familyKey = "families";

export const useFamilies = (): UseQueryResult<Family[]> => useApiGet<Family[]>([familyKey], "api/families", { staleTime: 1000 * 60 * 5 });

export const useFamily = (id: string): UseQueryResult<Family> => useApiGet<Family>([familyKey, id], `api/families/${id}`, { enabled: !!id });

export const useCreateFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<Family, null, Family>(() => "api/families", {
        onMutate: ([_variables, data]) => {
            let allFamilies = queryClient.getQueryData<Family[]>([familyKey]);
            if (!allFamilies) {
                console.warn("Query Cache is missing Families");
                return;
            }

            allFamilies.push(data);
            allFamilies = allFamilies.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Family[]>([familyKey], allFamilies);
        },
        onError: (_error, [_variables, _data]) => {
            queryClient.invalidateQueries({ queryKey: [familyKey] });
        }
    });

    return (family: Family) =>
        toast.promise(mutateAsync([null, family]), { pending: "Creating family", success: "Family created", error: "Failed to create family" });
}

export const useUpdateFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<Family, string, Family>((id) => `api/families/${id}`, {
        onMutate: ([_variables, data]) => {
            let allFamilies = queryClient.getQueryData<Family[]>([familyKey]);
            if (!allFamilies) {
                console.warn("Query Cache is missing Families");
                return;
            }

            allFamilies = allFamilies.map((family) => family.id === data.id ? data : family);
            allFamilies = allFamilies.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Family[]>([familyKey], allFamilies);

            queryClient.setQueryData<Family>([familyKey, data.id], data);
        },
        onError: (_error, [id, _data]) => {
            queryClient.invalidateQueries({ queryKey: [familyKey] });
            queryClient.invalidateQueries({ queryKey: [familyKey, id] });
        }
    });

    return (family: Family) =>
        toast.promise(mutateAsync([family.id, family]), { pending: "Updating family", success: "Family updated", error: "Failed to update family" });
}
