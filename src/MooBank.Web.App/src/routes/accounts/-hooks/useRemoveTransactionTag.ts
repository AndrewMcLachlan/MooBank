import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction, Tag } from "api/types.gen";
import { State } from "store/state";
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
                queryClient.setQueryData<PagedResult<Transaction>>(queryKey, transactions);
            }
        }

        rawMutate({ path: { instrumentId: variables.accountId, id: variables.transactionId, tagId: variables.tag.id } });
    };

    return { mutate };
}
