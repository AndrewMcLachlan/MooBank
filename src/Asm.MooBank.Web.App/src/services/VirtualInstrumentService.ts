import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { InstitutionAccount, InstrumentId, AccountList, VirtualAccount } from "../models";
import { accountsKey } from "./AccountService";
import { formattedAccountsKey } from "./InstrumentsService";
import { emptyGuid, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";

interface VirtualAccountVariables {
    accountId: InstrumentId;
    virtualAccountId: InstrumentId;
}

export const useVirtualAccounts = (accountId: InstrumentId): UseQueryResult<VirtualAccount[]> => useApiGet<VirtualAccount[]>(["virtualaccount", accountId], `api/instruments/${accountId}/virtual`);

export const useVirtualAccount = (accountId: InstrumentId, virtualAccountId: InstrumentId) => useApiGet<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }], `api/instruments/${accountId}/virtual/${virtualAccountId}`);

export const useCreateVirtualAccount = () => {

    const queryClient = useQueryClient();

    const { mutate } = useApiPost<VirtualAccount, InstrumentId, VirtualAccount>((accountId) => `api/instruments/${accountId}/virtual`, {
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    const create = (accountId: InstrumentId, virtualAccount: VirtualAccount) => {
        mutate([accountId, virtualAccount]);
    };

    return create;
}

export const useUpdateVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<InstitutionAccount, VirtualAccountVariables, VirtualAccount>(({ accountId, virtualAccountId }) => `api/instruments/${accountId}/virtual/${virtualAccountId}`, {
        onSettled: (_data, _error, [{ accountId, virtualAccountId }]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", { accountId, virtualAccountId }] });
        }
    });

    const update = (accountId: InstrumentId, account: VirtualAccount) => {
        mutate([{ accountId, virtualAccountId: account.id }, account]);
    };

    return update;
}

export const useUpdateVirtualAccountBalance = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<VirtualAccount, VirtualAccountVariables, { balance: number }>(({ accountId, virtualAccountId }) => `api/instruments/${accountId}/virtual/${virtualAccountId}/balance`, {

        onMutate: async ([{ accountId, virtualAccountId }, { balance }]) => {

            await queryClient.cancelQueries({ queryKey: [accountsKey] });
            await queryClient.cancelQueries({ queryKey: [formattedAccountsKey] });

            const accounts = queryClient.getQueryData<AccountList>([formattedAccountsKey]);

            if (!accounts) return;

            const account = accounts.groups.flatMap(g => g.instruments).find(a => a.id === accountId);
            if (!account) return;
            const vAccount = account.virtualInstruments.find(a => a.id === virtualAccountId);
            if (!vAccount) return;
            const remaining = account.virtualInstruments.find(a => a.id === emptyGuid);
            if (!remaining) return;
            //const others = account.virtualInstruments.filter(a => a.id !== emptyGuid);

            const difference = vAccount.currentBalance - balance;

            vAccount.currentBalance = balance;
            remaining.currentBalance += difference;
            account.virtualAccountRemainingBalance = remaining.currentBalance;
            //instruments.accounts = [];
            accounts.total = 0;

            queryClient.setQueryData<AccountList>([formattedAccountsKey], { ...accounts });

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [formattedAccountsKey] });
        },
    });

    const update = (accountId: string, virtualAccountId: InstrumentId, balance: number) => {

        mutate([{ accountId, virtualAccountId }, { balance }]);
    };

    return update;
}
