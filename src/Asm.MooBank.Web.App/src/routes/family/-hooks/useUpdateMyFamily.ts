import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getMyFamilyQueryKey, updateMyFamilyMutation } from "api/@tanstack/react-query.gen";
import type { Family } from "api/types.gen";
import { toast } from "react-toastify";

export const useUpdateMyFamily = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateMyFamilyMutation(),
        onSuccess: (data) => {
            queryClient.setQueryData<Family>(getMyFamilyQueryKey(), data);
        },
        onError: () => {
            queryClient.invalidateQueries({ queryKey: getMyFamilyQueryKey() });
        },
    });

    return (family: Family) =>
        toast.promise(mutateAsync({ body: family as any }), { pending: "Updating family", success: "Family updated", error: "Failed to update family" });
}
