import { useMutation, useQueryClient } from "@tanstack/react-query";
import { closeInstitutionAccountMutation, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "react-toastify";

export const useCloseInstitutionAccount = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...closeInstitutionAccountMutation(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, institutionAccountId: string) =>
            toast.promise(mutateAsync({ path: { instrumentId: accountId, id: institutionAccountId } }), { pending: "Closing institution account", success: "Institution account closed", error: "Failed to close institution account" }),
        ...rest,
    };
};
