import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { LogicalAccount, InstrumentId, AccountList, VirtualAccount, CreateVirtualInstrument } from "../models";
import { accountsKey } from "./AccountService";
import { formattedAccountsKey } from "./InstrumentsService";
import { useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { toast } from "react-toastify";

interface VirtualAccountVariables {
    accountId: InstrumentId;
    virtualAccountId: InstrumentId;
}

export const useVirtualAccounts = (accountId: InstrumentId): UseQueryResult<VirtualAccount[]> => useApiGet<VirtualAccount[]>(["virtualaccount", accountId], `api/instruments/${accountId}/virtual`);

export const useVirtualAccount = (accountId: InstrumentId, virtualAccountId: InstrumentId) => useApiGet<VirtualAccount>(["virtualaccount", { accountId, virtualAccountId }], `api/instruments/${accountId}/virtual/${virtualAccountId}`);

export const useCreateVirtualAccount = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<VirtualAccount, InstrumentId, CreateVirtualInstrument>((accountId) => `api/instruments/${accountId}/virtual`, {
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
        }
    });

    return {
        mutateAsync: (accountId: InstrumentId, virtualAccount: CreateVirtualInstrument) =>
            toast.promise(mutateAsync([accountId, virtualAccount]), { pending: "Creating virtual account", success: "Virtual account created", error: "Failed to create virtual account" }),
        ...rest,
    };
}

export const useUpdateVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<LogicalAccount, VirtualAccountVariables, VirtualAccount>(({ accountId, virtualAccountId }) => `api/instruments/${accountId}/virtual/${virtualAccountId}`, {
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

            const difference = vAccount.currentBalance - balance;

            vAccount.currentBalance = balance;
            account.remainingBalance += difference;
            // TODO: Update the local currency balance if needed
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
