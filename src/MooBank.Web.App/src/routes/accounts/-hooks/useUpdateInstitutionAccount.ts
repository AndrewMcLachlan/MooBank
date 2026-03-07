import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateInstitutionAccountMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { UpdateInstitutionAccount } from "api/types.gen";
import { toast } from "react-toastify";

export const useUpdateInstitutionAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateInstitutionAccountMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, institutionAccountId: string, institutionAccount: UpdateInstitutionAccount) =>
            toast.promise(mutateAsync({ body: institutionAccount, path: { instrumentId: accountId, id: institutionAccountId } }), { pending: "Updating institution account", success: "Institution account updated", error: "Failed to update institution account" }),
        ...rest,
    };
};
