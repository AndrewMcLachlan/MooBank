import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { Group } from "../models";

export const accountGroupsKey = "groups";

export const useAccountGroups = (): UseQueryResult<Group[]> => useApiGet<Group[]>([accountGroupsKey], "api/groups");

export const useAccountGroup = (id: string) => useApiGet<Group>([accountGroupsKey, id], `api/groups/${id}`);

export const useCreateAccountGroup = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPost<Group, null, Group>(() => "api/groups", {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountGroupsKey]});
        }
    });

    const create = (accountGroup: Group) => {
        mutate([null, accountGroup]);
    };

    return create;
}



export const useUpdateAccountGroup = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<Group, string, Group>((id) => `api/groups/${id}`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountGroupsKey]});
        }
    });

    const update = (accountGroup: Group) => {
        mutate([accountGroup.id, accountGroup]);
    };

    return update;
}
