import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { VirtualInstrument } from "api/types.gen";
import {
    getVirtualInstrumentsQueryKey,
    updateVirtualInstrumentMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateVirtualInstrument = () => {
    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...updateVirtualInstrumentMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        }
    });

    const update = (accountId: string, virtualInstrument: VirtualInstrument) => {
        toast.promise(mutateAsync({ body: virtualInstrument as any, path: { instrumentId: accountId, virtualInstrumentId: virtualInstrument.id } } as any), { pending: "Updating virtual account", success: "Virtual account updated", error: "Failed to update virtual account" });
    };

    return update;
}
