import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { VirtualInstrument, CreateVirtualInstrument, InstrumentsList } from "api/types.gen";
import { accountsQueryKey } from "./AccountService";
import { formattedAccountsQueryKey } from "./InstrumentsService";
import {
    getVirtualInstrumentsOptions,
    getVirtualInstrumentsQueryKey,
    getVirtualInstrumentOptions,
    createVirtualInstrumentMutation,
    updateVirtualInstrumentMutation,
    updateVirtualInstrumentBalanceMutation,
    deleteVirtualInstrumentMutation,
} from "api/@tanstack/react-query.gen";
import { toast } from "react-toastify";

export const useVirtualInstruments = (accountId: string) => useQuery({
    ...getVirtualInstrumentsOptions({ path: { instrumentId: accountId } }),
});

export const useVirtualInstrument = (accountId: string, virtualInstrumentId: string) => useQuery({
    ...getVirtualInstrumentOptions({ path: { instrumentId: accountId, virtualInstrumentId } }),
});

export const useCreateVirtualInstrument = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createVirtualInstrumentMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
        }
    });

    return {
        mutateAsync: (accountId: string, virtualAccount: CreateVirtualInstrument) =>
            toast.promise(mutateAsync({ body: virtualAccount, path: { instrumentId: accountId } }), { pending: "Creating virtual account", success: "Virtual account created", error: "Failed to create virtual account" }),
        ...rest,
    };
}

export const useUpdateVirtualInstrument = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateVirtualInstrumentMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        }
    });

    const update = (accountId: string, virtualInstrument: VirtualInstrument) => {
        mutate({ body: virtualInstrument as any, path: { instrumentId: accountId, virtualInstrumentId: virtualInstrument.id } } as any);
    };

    return update;
}

export const useUpdateVirtualInstrumentBalance = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateVirtualInstrumentBalanceMutation(),

        onMutate: async (variables) => {

            await queryClient.cancelQueries({ queryKey: accountsQueryKey() });
            await queryClient.cancelQueries({ queryKey: formattedAccountsQueryKey() });

            const accounts = queryClient.getQueryData<InstrumentsList>(formattedAccountsQueryKey());

            if (!accounts) return;

            const vars = variables as any;
            const account = accounts.groups.flatMap(g => g.instruments).find(a => a.id === vars.path!.instrumentId);
            if (!account) return;
            const vAccount = account.virtualInstruments.find(a => a.id === vars.path!.virtualInstrumentId);
            if (!vAccount) return;

            const difference = Number(vAccount.currentBalance) - vars.body.balance;

            vAccount.currentBalance = vars.body.balance;
            (account as any).remainingBalance += difference;
            // TODO: Update the local currency balance if needed
            accounts.total = 0;

            queryClient.setQueryData<InstrumentsList>(formattedAccountsQueryKey(), { ...accounts });

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: formattedAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        },
    });

    const update = (accountId: string, virtualInstrumentId: string, balance: number) => {

        mutate({ body: { balance }, path: { instrumentId: accountId, virtualInstrumentId } } as any);
    };

    return update;
}

export const useCloseVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...deleteVirtualInstrumentMutation(),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: formattedAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        }
    });

    return {
        mutateAsync: (accountId: string, virtualInstrumentId: string) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, virtualInstrumentId } } as any), { pending: "Closing virtual account", success: "Virtual account closed", error: "Failed to close virtual account" }),
        ...rest,
    };
}
