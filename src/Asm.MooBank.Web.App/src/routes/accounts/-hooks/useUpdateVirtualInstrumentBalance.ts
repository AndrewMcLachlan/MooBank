import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { InstrumentsList } from "api/types.gen";
import {
    getVirtualInstrumentsQueryKey,
    updateVirtualInstrumentBalanceMutation,
    getAccountsQueryKey,
    getFormattedInstrumentsListQueryKey,
} from "api/@tanstack/react-query.gen";

export const useUpdateVirtualInstrumentBalance = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateVirtualInstrumentBalanceMutation(),

        onMutate: async (variables) => {

            await queryClient.cancelQueries({ queryKey: getAccountsQueryKey() });
            await queryClient.cancelQueries({ queryKey: getFormattedInstrumentsListQueryKey() });

            const accounts = queryClient.getQueryData<InstrumentsList>(getFormattedInstrumentsListQueryKey());

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

            queryClient.setQueryData<InstrumentsList>(getFormattedInstrumentsListQueryKey(), { ...accounts });

            return accounts;
        },

        onError: (e) => {
            console.error(e);
        },

        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getFormattedInstrumentsListQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        },
    });

    const update = (accountId: string, virtualInstrumentId: string, balance: number) => {

        mutate({ body: { balance }, path: { instrumentId: accountId, virtualInstrumentId } } as any);
    };

    return update;
}
