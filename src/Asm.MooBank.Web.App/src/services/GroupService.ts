import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { Group } from "../models";

export const groupsKey = "groups";

export const useGroups = (): UseQueryResult<Group[]> => useApiGet<Group[]>([groupsKey], "api/groups");

export const useGroup = (id: string) => useApiGet<Group>([groupsKey, id], `api/groups/${id}`);

export const useCreateGroup = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPost<Group, null, Group>(() => "api/groups", {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [groupsKey]});
        }
    });

    const create = (group: Group) => {
        mutate([null, group]);
    };

    return create;
}



export const useUpdateGroup = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<Group, string, Group>((id) => `api/groups/${id}`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [groupsKey]});
        }
    });

    const update = (group: Group) => {
        mutate([group.id, group]);
    };

    return update;
}
