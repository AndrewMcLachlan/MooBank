import { useQueryClient } from "@tanstack/react-query";
import { useApiGet, useApiPatch } from "@andrewmclachlan/mooapp";
import { User } from "models/User";

export const usersKey = "users";

export const useUser = () => useApiGet<User>([usersKey, "me"], `api/users/me`);

export const useUpdateUser = () => {
    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<User, null, User>(() => "api/users/me", {
        onSettled: (_data,_error,[]) => {
            queryClient.invalidateQueries({ queryKey: [usersKey]});
        }
    });

    const update = (user: User) => {
        mutate([null, user]);
    };

    return update;
}
