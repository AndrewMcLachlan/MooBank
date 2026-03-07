import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    getVirtualInstrumentsQueryKey,
    deleteVirtualInstrumentMutation,
    getAccountsQueryKey,
    getFormattedInstrumentsListQueryKey,
} from "api/@tanstack/react-query.gen";
import { toast } from "react-toastify";

export const useCloseVirtualAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...deleteVirtualInstrumentMutation(),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getFormattedInstrumentsListQueryKey() });
            queryClient.invalidateQueries({ queryKey: getVirtualInstrumentsQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
        }
    });

    return {
        mutateAsync: (accountId: string, virtualInstrumentId: string) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, virtualInstrumentId } } as any), { pending: "Closing virtual account", success: "Virtual account closed", error: "Failed to close virtual account" }),
        ...rest,
    };
}
