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

            const queryKey = getFormattedInstrumentsListQueryKey();

            await queryClient.cancelQueries({ queryKey: getAccountsQueryKey() });
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<InstrumentsList>(queryKey);
            if (!previous) return { previous: undefined };

            const vars = variables as any;
            const account = previous.groups.flatMap(g => g.instruments).find(a => a.id === vars.path!.instrumentId);
            const vAccount = account?.virtualInstruments.find(a => a.id === vars.path!.virtualInstrumentId);
            if (!account || !vAccount) return { previous: undefined };

            const difference = Number(vAccount.currentBalance) - vars.body.balance;

            const next: InstrumentsList = {
                ...previous,
                groups: previous.groups.map(group => ({
                    ...group,
                    instruments: group.instruments.map(instrument => instrument.id !== vars.path!.instrumentId ? instrument : {
                        ...instrument,
                        remainingBalance: ((instrument as any).remainingBalance ?? 0) + difference,
                        virtualInstruments: instrument.virtualInstruments.map(vi => vi.id !== vars.path!.virtualInstrumentId ? vi : {
                            ...vi,
                            currentBalance: vars.body.balance,
                        }),
                    } as any),
                })),
            };

            queryClient.setQueryData<InstrumentsList>(queryKey, next);

            return { previous };
        },

        onError: (_error, _variables, context: any) => {
            if (context?.previous) {
                queryClient.setQueryData(getFormattedInstrumentsListQueryKey(), context.previous);
            }
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
