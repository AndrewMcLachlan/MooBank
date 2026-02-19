import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getUserOptions, getUserQueryKey, updateUserMutation } from "api/@tanstack/react-query.gen";
import type { User } from "api/types.gen";
import { toast } from "react-toastify";

export const useUser = () => useQuery({ ...getUserOptions() });

export const useUpdateUser = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateUserMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getUserQueryKey() });
        },
    });

    return (user: User) =>
        toast.promise(mutateAsync({ body: user as any }), { pending: "Updating your profile", success: "Profile updated", error: "Failed to update your profile" });
}
