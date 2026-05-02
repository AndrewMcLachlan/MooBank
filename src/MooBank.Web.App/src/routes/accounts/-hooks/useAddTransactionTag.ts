import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import type { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction, Tag } from "api/types.gen";
import type { State } from "store/state";
import {
    getTransactionsQueryKey,
    addTagMutation,
} from "api/@tanstack/react-query.gen";
import { buildTransactionsQueryKey } from "./transactionKeys";

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutate: rawMutate } = useMutation({
        ...addTagMutation(),
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
                transaction.tags.push(variables.tag);
                // Mirror backend Transaction.AddOrUpdateSplit: tags live on splits, so keep splits[0] in sync
                // so the detail modal — which initialises from transaction.splits — sees the new tag and
                // doesn't overwrite it on save.
                const simpleTag = { id: variables.tag.id, name: variables.tag.name };
                const firstSplit = transaction.splits?.[0];
                if (firstSplit) {
                    if (!firstSplit.tags.some(t => t.id === simpleTag.id)) {
                        firstSplit.tags.push(simpleTag);
                    }
                } else {
                    transaction.splits = [{
                        id: crypto.randomUUID(),
                        tags: [simpleTag],
                        amount: Math.abs(transaction.amount),
                        offsetBy: [],
                    }];
                }
                queryClient.setQueryData<PagedResult<Transaction>>(queryKey, transactions);
            }
        }

        rawMutate({ path: { instrumentId: variables.accountId, id: variables.transactionId, tagId: variables.tag.id } });
    };

    return { mutate };
}
