import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { VirtualInstrument } from "api/types.gen";
import {
    getVirtualInstrumentsQueryKey,
    updateVirtualInstrumentMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";

export const useUpdateVirtualInstrument = () => {
    const queryClient = useQueryClient();

    const { mutate } = useMutation({
        ...updateVirtualInstrumentMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        }
    });

    const update = (accountId: string, virtualInstrument: VirtualInstrument) => {
        mutate({ body: virtualInstrument as any, path: { instrumentId: accountId, virtualInstrumentId: virtualInstrument.id } } as any);
    };

    return update;
}
