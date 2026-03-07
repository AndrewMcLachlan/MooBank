import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createInstitutionAccountMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { CreateInstitutionAccount } from "api/types.gen";
import { toast } from "react-toastify";

export const useCreateInstitutionAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createInstitutionAccountMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, institutionAccount: CreateInstitutionAccount) =>
            toast.promise(mutateAsync({ body: institutionAccount, path: { instrumentId: accountId } }), { pending: "Creating institution account", success: "Institution account created", error: "Failed to create institution account" }),
        ...rest,
    };
};
