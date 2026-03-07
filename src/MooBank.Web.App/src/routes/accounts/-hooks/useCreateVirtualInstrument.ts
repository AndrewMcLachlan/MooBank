import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateVirtualInstrument } from "api/types.gen";
import {
    createVirtualInstrumentMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import { toast } from "react-toastify";

export const useCreateVirtualInstrument = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createVirtualInstrumentMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        }
    });

    return {
        mutateAsync: (accountId: string, virtualAccount: CreateVirtualInstrument) =>
            toast.promise(mutateAsync({ body: virtualAccount, path: { instrumentId: accountId } }), { pending: "Creating virtual account", success: "Virtual account created", error: "Failed to create virtual account" }),
        ...rest,
    };
}
