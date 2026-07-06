import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import type { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction, Tag } from "api/types.gen";
import type { State } from "store/state";
import { addTag } from "api/sdk.gen";
import { buildTransactionsQueryKey, invalidateTransactionLists } from "./transactionKeys";

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutate } = useMutation({
        mutationFn: (variables: { accountId: string, transactionId: string, tag: Tag }) =>
            addTag({ path: { instrumentId: variables.accountId, id: variables.transactionId, tagId: variables.tag.id }, throwOnError: true }),
        onMutate: async (variables) => {

            const queryKey = buildTransactionsQueryKey(variables.accountId, filter, pageSize, currentPage, sortField, sortDirection);
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<PagedResult<Transaction>>(queryKey);
            if (!previous?.results) return { queryKey, previous: undefined };

            const next: PagedResult<Transaction> = {
                ...previous,
                results: previous.results.map(tr => {
                    if (tr.id !== variables.transactionId) return tr;

                    // Mirror backend Transaction.AddOrUpdateSplit: tags live on splits, so keep splits[0] in sync
                    // so the detail modal — which initialises from transaction.splits — sees the new tag and
                    // doesn't overwrite it on save.
                    const simpleTag = { id: variables.tag.id, name: variables.tag.name };
                    const splits = tr.splits?.length
                        ? tr.splits.map((s, i) => i === 0 && !s.tags.some(t => t.id === simpleTag.id)
                            ? { ...s, tags: [...s.tags, simpleTag] }
                            : s)
                        : [{
                            id: crypto.randomUUID(),
                            tags: [simpleTag],
                            amount: Math.abs(tr.amount),
                            offsetBy: [],
                        }];

                    return { ...tr, tags: [...tr.tags, variables.tag], splits };
                }),
            };
            queryClient.setQueryData<PagedResult<Transaction>>(queryKey, next);

            return { queryKey, previous };
        },
        onError: (_error, _variables, context) => {
            if (context?.previous) {
                queryClient.setQueryData(context.queryKey, context.previous);
            }
        },
        onSettled: () => invalidateTransactionLists(queryClient),
    });

    return { mutate };
}
