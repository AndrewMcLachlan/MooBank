import { useMutation, MutationOptions, useQueryClient } from "@tanstack/react-query";
import {
    importMutation,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import { toast } from "react-toastify";

export const useImportTransactions = () => {

    const client = useQueryClient();

    const { mutateAsync } = useMutation({
        ...importMutation(),
        onSuccess: () => {
            client.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    const importTransactions = (instrumentId: string, institutionAccountId: string, file: File, options?: MutationOptions) => {
        return toast.promise(mutateAsync({ body: { file } as any, path: { instrumentId, accountId: institutionAccountId } }, options as any), { pending: "Uploading transactions", success: "Import started", error: "Failed to import transactions" });
    }

    return importTransactions;
}
