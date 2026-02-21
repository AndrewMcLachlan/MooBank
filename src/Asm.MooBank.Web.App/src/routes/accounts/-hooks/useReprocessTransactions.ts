import { useMutation, MutationOptions, useQueryClient } from "@tanstack/react-query";
import {
    reprocessMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import { toast } from "react-toastify";

export const useReprocessTransactions = () => {

    const client = useQueryClient();

    const { mutateAsync } = useMutation({
        ...reprocessMutation(),
        onSuccess: () => {
            client.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    const reprocessTransactions = (instrumentId: string, institutionAccountId: string, options?: MutationOptions) => {
        return toast.promise(mutateAsync({ path: { instrumentId, accountId: institutionAccountId } }, options as any), { pending: "Requesting reprocessing", success: "Reprocessing started", error: "Failed to reprocess transactions" });
    }

    return reprocessTransactions;
}
