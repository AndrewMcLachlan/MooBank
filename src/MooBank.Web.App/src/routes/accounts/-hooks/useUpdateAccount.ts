import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateAccountMutation, getAccountQueryKey, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { LogicalAccount } from "api/types.gen";
import { toast } from "react-toastify";

export const useUpdateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateAccountMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getAccountQueryKey({ path: { instrumentId: (variables as any).path!.id } }) });
        },
    });

    return {
        mutateAsync: (account: LogicalAccount) =>
            toast.promise(mutateAsync({ body: account, path: { id: account.id } } as any), { pending: "Updating account", success: "Account updated", error: "Failed to update account" }),
        ...rest,
    };
};
