import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getMyFamilyOptions, getMyFamilyQueryKey, updateMyFamilyMutation, removeFamilyMemberMutation } from "api/@tanstack/react-query.gen";
import { UpdateFamily, Family as GenFamily } from "api/types.gen";

import { Family } from "models";
import { toast } from "react-toastify";

export const useMyFamily = () => useQuery({ ...getMyFamilyOptions() });

export const useUpdateMyFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateMyFamilyMutation(),
        onSuccess: (data) => {
            queryClient.setQueryData<GenFamily>(getMyFamilyQueryKey(), data);
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: getMyFamilyQueryKey() });
        },
    });

    return (family: Family) =>
        toast.promise(mutateAsync({ body: family as unknown as UpdateFamily }), { pending: "Updating family", success: "Family updated", error: "Failed to update family" });
}

export const useRemoveFamilyMember = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...removeFamilyMemberMutation(),
        onSuccess: (_data, variables) => {
            const family = queryClient.getQueryData<GenFamily>(getMyFamilyQueryKey());
            if (family) {
                queryClient.setQueryData<GenFamily>(getMyFamilyQueryKey(), {
                    ...family,
                    members: family.members?.filter(m => m.id !== variables.path!.userId)
                });
            }
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: getMyFamilyQueryKey() });
        },
    });

    return (userId: string) =>
        toast.promise(mutateAsync({ path: { userId } }), { pending: "Removing family member", success: "Family member removed", error: "Failed to remove family member" });
}
