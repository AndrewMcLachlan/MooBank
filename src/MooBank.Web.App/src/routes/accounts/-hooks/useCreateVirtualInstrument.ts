import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateVirtualInstrument } from "api/types.gen";
import {
    createVirtualInstrumentMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateVirtualInstrument = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createVirtualInstrumentMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        }
    });

    return {
        mutateAsync: (accountId: string, virtualInstrument: CreateVirtualInstrument) =>
            toast.promise(mutateAsync({ body: virtualInstrument, path: { instrumentId: accountId } }), { pending: "Creating virtual account", success: "Virtual account created", error: "Failed to create virtual account" }),
        ...rest,
    };
}
