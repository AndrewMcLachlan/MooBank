import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getMyFamilyQueryKey, removeFamilyMemberMutation } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "react-toastify";

export const useRemoveFamilyMember = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...removeFamilyMemberMutation(),
        onSuccess: (_data, variables) => {
            const family = queryClient.getQueryData<Family>(getMyFamilyQueryKey());
            if (family) {
                queryClient.setQueryData<Family>(getMyFamilyQueryKey(), {
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
