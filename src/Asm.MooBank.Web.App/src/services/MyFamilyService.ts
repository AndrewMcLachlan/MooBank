import { useApiDelete, useApiGet, useApiPatch } from "@andrewmclachlan/moo-app";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";

import { Family } from "models";
import { toast } from "react-toastify";

const myFamilyKey = "my-family";

export const useMyFamily = (): UseQueryResult<Family> => useApiGet<Family>([myFamilyKey], "api/families");

export const useUpdateMyFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useApiPatch<Family, null, Family>(() => "api/families", {
        onSuccess: (data) => {
            queryClient.setQueryData<Family>([myFamilyKey], data);
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: [myFamilyKey] });
        }
    });

    return (family: Family) =>
        toast.promise(mutateAsync([null, family]), { pending: "Updating family", success: "Family updated", error: "Failed to update family" });
}

export const useRemoveFamilyMember = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useApiDelete<string>((userId) => `api/families/members/${userId}`, {
        onSuccess: (_data, userId) => {
            const family = queryClient.getQueryData<Family>([myFamilyKey]);
            if (family) {
                queryClient.setQueryData<Family>([myFamilyKey], {
                    ...family,
                    members: family.members?.filter(m => m.id !== userId)
                });
            }
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: [myFamilyKey] });
        }
    });

    return (userId: string) =>
        toast.promise(mutateAsync(userId), { pending: "Removing family member", success: "Family member removed", error: "Failed to remove family member" });
}
