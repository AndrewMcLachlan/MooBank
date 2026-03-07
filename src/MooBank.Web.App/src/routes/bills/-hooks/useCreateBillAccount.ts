import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    createBillAccountMutation,
    getBillAccountsQueryKey,
    getBillAccountSummariesByTypeQueryKey,
} from "api/@tanstack/react-query.gen";
import type { CreateBillAccount } from "api/types.gen";
import { toast } from "react-toastify";

export const useCreateBillAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createBillAccountMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
            queryClient.invalidateQueries({ queryKey: getBillAccountSummariesByTypeQueryKey() });
        },
    });

    return {
        mutateAsync: (account: CreateBillAccount) =>
            toast.promise(mutateAsync({ body: account }), { pending: "Creating account", success: "Account created", error: "Failed to create account" }),
        ...rest,
    };
};
