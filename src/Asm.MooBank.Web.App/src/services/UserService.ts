import { useQueryClient } from "@tanstack/react-query";
import { useApiGet, useApiPatch } from "@andrewmclachlan/mooapp";
import { User } from "models/User";
import { toast } from "react-toastify";

export const usersKey = "users";

export const useUser = () => useApiGet<User>([usersKey, "me"], `api/users/me`);

export const useUpdateUser = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useApiPatch<User, null, User>(() => "api/users/me", {
        onSettled: (_data,_error) => {
            queryClient.invalidateQueries({ queryKey: [usersKey]});
        }
    });

    return (user: User) =>
        toast.promise(mutateAsync([null, user]), { pending: "Updating your profile", success: "Profile updated", error: "Failed to update your profile" });
}
