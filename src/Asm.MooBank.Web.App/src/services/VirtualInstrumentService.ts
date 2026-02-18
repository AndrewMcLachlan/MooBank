import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AccountList, InstrumentId, VirtualInstrument, CreateVirtualInstrument } from "../models";
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
import {
    CreateVirtualInstrument as GenCreateVirtualInstrument,
    UpdateVirtualInstrumentData,
} from "api/types.gen";
import { toast } from "react-toastify";

export const useVirtualInstruments = (accountId: InstrumentId) => useQuery({
    ...getVirtualInstrumentsOptions({ path: { instrumentId: accountId } }),
    select: (data) => data as unknown as VirtualInstrument[],
});

export const useVirtualInstrument = (accountId: InstrumentId, virtualInstrumentId: InstrumentId) => useQuery({
    ...getVirtualInstrumentOptions({ path: { instrumentId: accountId, virtualInstrumentId } }),
    select: (data) => data as unknown as VirtualInstrument,
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
        mutateAsync: (accountId: InstrumentId, virtualAccount: CreateVirtualInstrument) =>
            toast.promise(mutateAsync({ body: virtualAccount as unknown as GenCreateVirtualInstrument, path: { instrumentId: accountId } }), { pending: "Creating virtual account", success: "Virtual account created", error: "Failed to create virtual account" }),
        ...rest,
    };
}

export const useUpdateVirtualInstrument = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateVirtualInstrumentMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: variables.path!.instrumentId } }) });
        }
    });

    const update = (accountId: InstrumentId, virtualInstrument: VirtualInstrument) => {
        mutate({ body: virtualInstrument as unknown as UpdateVirtualInstrumentData["body"], path: { instrumentId: accountId, virtualInstrumentId: virtualInstrument.id } });
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

            const accounts = queryClient.getQueryData<AccountList>(formattedAccountsQueryKey());

            if (!accounts) return;

            const account = accounts.groups.flatMap(g => g.instruments).find(a => a.id === variables.path!.instrumentId);
            if (!account) return;
            const vAccount = account.virtualInstruments.find(a => a.id === variables.path!.virtualInstrumentId);
            if (!vAccount) return;

            const difference = vAccount.currentBalance - variables.body.balance;

            vAccount.currentBalance = variables.body.balance;
            account.remainingBalance += difference;
            // TODO: Update the local currency balance if needed
            accounts.total = 0;

            queryClient.setQueryData<AccountList>(formattedAccountsQueryKey(), { ...accounts });

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: accountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: formattedAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: variables.path!.instrumentId } }) });
        },
    });

    const update = (accountId: string, virtualInstrumentId: InstrumentId, balance: number) => {

        mutate({ body: { balance }, path: { instrumentId: accountId, virtualInstrumentId } });
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
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: variables.path!.instrumentId } }) });
        }
    });

    return {
        mutateAsync: (accountId: InstrumentId, virtualInstrumentId: InstrumentId) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, virtualInstrumentId } }), { pending: "Closing virtual account", success: "Virtual account closed", error: "Failed to close virtual account" }),
        ...rest,
    };
}
