import { useQueryClient, UseQueryResult } from "@tanstack/react-query";
import { LogicalAccount, InstrumentId, AccountList, VirtualInstrument, CreateVirtualInstrument, UpdateVirtualInstrument } from "../models";
import { accountsKey } from "./AccountService";
import { formattedAccountsKey } from "./InstrumentsService";
import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { toast } from "react-toastify";

interface VirtualInstrumentVariables {
    accountId: InstrumentId;
    virtualInstrumentId: InstrumentId;
}

const virtualInstrumentsKey = "virtualinstruments";

export const useVirtualInstruments = (accountId: InstrumentId): UseQueryResult<VirtualInstrument[]> => useApiGet<VirtualInstrument[]>([virtualInstrumentsKey, accountId], `api/instruments/${accountId}/virtual`);

export const useVirtualInstrument = (accountId: InstrumentId, virtualInstrumentId: InstrumentId) => useApiGet<VirtualInstrument>([virtualInstrumentsKey, accountId, virtualInstrumentId], `api/instruments/${accountId}/virtual/${virtualInstrumentId}`);
export const useCreateVirtualInstrument = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<VirtualInstrument, InstrumentId, CreateVirtualInstrument>((accountId) => `api/instruments/${accountId}/virtual`, {
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

export const useUpdateVirtualInstrument = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<LogicalAccount, VirtualInstrumentVariables, UpdateVirtualInstrument>(({ accountId, virtualInstrumentId }) => `api/instruments/${accountId}/virtual/${virtualInstrumentId}`, {
        onSettled: (_data, _error, [{ accountId }]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [virtualInstrumentsKey, accountId] });
        }
    });

    const update = (accountId: InstrumentId, virtualInstrument: VirtualInstrument) => {
        mutate([{ accountId, virtualInstrumentId: virtualInstrument.id }, virtualInstrument]);
    };

    return update;
}

export const useUpdateVirtualInstrumentBalance = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiPatch<VirtualInstrument, VirtualInstrumentVariables, { balance: number }>(({ accountId, virtualInstrumentId }) => `api/instruments/${accountId}/virtual/${virtualInstrumentId}/balance`, {

        onMutate: async ([{ accountId, virtualInstrumentId }, { balance }]) => {

            await queryClient.cancelQueries({ queryKey: [accountsKey] });
            await queryClient.cancelQueries({ queryKey: [formattedAccountsKey] });

            const accounts = queryClient.getQueryData<AccountList>([formattedAccountsKey]);

            if (!accounts) return;

            const account = accounts.groups.flatMap(g => g.instruments).find(a => a.id === accountId);
            if (!account) return;
            const vAccount = account.virtualInstruments.find(a => a.id === virtualInstrumentId);
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

        onSettled: (_data, _error, [{ accountId }]) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [formattedAccountsKey] });
            queryClient.invalidateQueries({ queryKey: [virtualInstrumentsKey, accountId] });
        },
    });

    const update = (accountId: string, virtualInstrumentId: InstrumentId, balance: number) => {

        mutate([{ accountId, virtualInstrumentId }, { balance }]);
    };

    return update;
}

export const useCloseVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiDelete<VirtualInstrumentVariables>(({ accountId, virtualInstrumentId }) => `api/instruments/${accountId}/virtual/${virtualInstrumentId}`, {
        onSuccess: (_data, { accountId }) => {
            queryClient.invalidateQueries({ queryKey: [accountsKey] });
            queryClient.invalidateQueries({ queryKey: [formattedAccountsKey] });
            queryClient.invalidateQueries({ queryKey: ["virtualaccount", accountId] });
        }
    });

    return {
        mutateAsync: (accountId: InstrumentId, virtualInstrumentId: InstrumentId) =>
            toast.promise(mutateAsync({ accountId, virtualInstrumentId }), { pending: "Closing virtual account", success: "Virtual account closed", error: "Failed to close virtual account" }),
        ...rest,
    };
}
