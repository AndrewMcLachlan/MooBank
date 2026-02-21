import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    createBillMutation,
    getAllBillsQueryKey,
    getBillsForAnAccountQueryKey,
    getBillAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import type { CreateBill } from "helpers/bills";
import { toast } from "react-toastify";

export const useCreateBill = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createBillMutation(),
        onSettled: (_data, _error, variables) => {
            queryClient.invalidateQueries({ queryKey: getAllBillsQueryKey({ query: {} as any }) });
            queryClient.invalidateQueries({ queryKey: getBillsForAnAccountQueryKey({ path: { instrumentId: (variables as any).path!.instrumentId } }) });
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, bill: CreateBill) =>
            toast.promise(mutateAsync({ body: bill as any, path: { instrumentId: accountId } } as any), { pending: "Creating bill", success: "Bill created", error: "Failed to create bill" }),
        ...rest,
    };
};
