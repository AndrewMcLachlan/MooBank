import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import type { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction, Tag } from "api/types.gen";
import type { State } from "store/state";
import {
    getTransactionsQueryKey,
    removeTagMutation,
} from "api/@tanstack/react-query.gen";
import { buildTransactionsQueryKey } from "./transactionKeys";

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutate: rawMutate } = useMutation({
        ...removeTagMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
        }
    });

    const mutate = (variables: { accountId: string, transactionId: string, tag: Tag }) => {

        const queryKey = buildTransactionsQueryKey(variables.accountId, filter, pageSize, currentPage, sortField, sortDirection);
        const transactions = { ...queryClient.getQueryData<PagedResult<Transaction>>(queryKey) };
        if (transactions?.results) {
            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (transaction) {
                transaction.tags = transaction.tags.filter(t => t.id !== variables.tag.id);
                // Mirror backend Transaction.UpdateOrRemoveSplit: drop the tag from the split that holds
                // it; if that empties the split and there's more than one, drop the split.
                const splits = transaction.splits ?? [];
                const splitIndex = splits.findIndex(s => s.tags.some(t => t.id === variables.tag.id));
                if (splitIndex !== -1) {
                    const split = splits[splitIndex];
                    split.tags = split.tags.filter(t => t.id !== variables.tag.id);
                    if (split.tags.length === 0 && splits.length > 1) {
                        transaction.splits = splits.filter((_, i) => i !== splitIndex);
                    }
                }
                queryClient.setQueryData<PagedResult<Transaction>>(queryKey, transactions);
            }
        }

        rawMutate({ path: { instrumentId: variables.accountId, id: variables.transactionId, tagId: variables.tag.id } });
    };

    return { mutate };
}
