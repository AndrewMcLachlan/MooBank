import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    updateBillMutation,
    getAllBillsQueryKey,
    getBillQueryKey,
    getBillsForAnAccountQueryKey,
    getBillAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import type { CreateBill } from "models/bills";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateBill = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateBillMutation(),
        onSettled: (_data, _error, variables) => {
            const path = (variables as any).path!;
            queryClient.invalidateQueries({ queryKey: getAllBillsQueryKey({ query: {} as any }) });
            queryClient.invalidateQueries({ queryKey: getBillsForAnAccountQueryKey({ path: { instrumentId: path.instrumentId } }) });
            // The drawer reads the bill through its own query, so an edit made from it has to
            // refresh that entry as well as the lists it appears in.
            queryClient.invalidateQueries({ queryKey: getBillQueryKey({ path: { instrumentId: path.instrumentId, id: path.id } }) });
            queryClient.invalidateQueries({ queryKey: getBillAccountsQueryKey() });
        },
    });

    return {
        mutateAsync: (accountId: string, id: number, bill: CreateBill) =>
            toast.promise(mutateAsync({ body: bill as any, path: { instrumentId: accountId, id } } as any), { pending: "Saving bill", success: "Bill saved", error: "Failed to save bill" }),
        ...rest,
    };
};
