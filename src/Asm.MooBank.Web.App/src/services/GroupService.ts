import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { Group } from "../models";

export const groupsKey = "groups";

export const useGroups = (): UseQueryResult<Group[]> => useApiGet<Group[]>([groupsKey], "api/groups");

export const useGroup = (id: string) => useApiGet<Group>([groupsKey, id], `api/groups/${id}`);

export const useCreateGroup = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<Group, null, Group>(() => "api/groups", {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [groupsKey] });
        }
    });

    return {
        mutateAsync: (group: Group) => {
            mutateAsync([null, group]);
        }, ...rest
    };
}

export const useUpdateGroup = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPatch<Group, string, Group>((id) => `api/groups/${id}`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [groupsKey] });
        }
    });

    return {
        mutateAsync: (group: Group) => {
            mutateAsync([group.id, group]);
        }, ...rest
    };
}
