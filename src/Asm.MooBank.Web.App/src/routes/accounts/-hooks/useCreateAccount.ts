import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createAccountMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { CreateAccount } from "api/types.gen";
import { toast } from "react-toastify";

export const useCreateAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createAccountMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (account: CreateAccount) =>
            toast.promise(mutateAsync({ body: account }), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};
