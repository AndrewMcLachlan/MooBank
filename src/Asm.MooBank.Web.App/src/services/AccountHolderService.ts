import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import { AccountHolder } from "models/AccountHolder";

export const accountHoldersKey = "account-holders";

export const useAccountHolder = () => useApiGet<AccountHolder>([accountHoldersKey, "me"], `api/account-holders/me`);

export const useUpdateAccountHolder = () => {
    const queryClient = useQueryClient();

    const { mutate} = useApiPatch<AccountHolder, null, AccountHolder>(() => "api/account-holders/me", {
        onSettled: (_data,_error,[]) => {
            queryClient.invalidateQueries({ queryKey: [accountHoldersKey]});
        }
    });

    const update = (user: AccountHolder) => {
        mutate([null, user]);
    };

    return update;
}
