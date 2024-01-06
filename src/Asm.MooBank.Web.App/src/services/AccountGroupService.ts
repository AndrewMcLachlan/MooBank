import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { AccountGroup } from "../models";

export const accountGroupsKey = "account-groups";

export const useAccountGroups = (): UseQueryResult<AccountGroup[]> => useApiGet<AccountGroup[]>([accountGroupsKey], "api/account-groups");

export const useAccountGroup = (id: string) => useApiGet<AccountGroup>([accountGroupsKey, id], `api/account-groups/${id}`);

export const useCreateAccountGroup = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPost<AccountGroup, null, AccountGroup>(() => "api/account-groups", {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountGroupsKey]});
        }
    });

    const create = (accountGroup: AccountGroup) => {
        mutate([null, accountGroup]);
    };

    return create;
}



export const useUpdateAccountGroup = () => {

    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<AccountGroup, string, AccountGroup>((id) => `api/account-groups/${id}`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountGroupsKey]});
        }
    });

    const update = (accountGroup: AccountGroup) => {
        mutate([accountGroup.id, accountGroup]);
    };

    return update;
}

