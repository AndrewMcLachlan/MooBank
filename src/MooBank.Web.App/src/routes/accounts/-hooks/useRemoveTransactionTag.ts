import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import type { PagedResult } from "@andrewmclachlan/moo-ds";
import type { Transaction, Tag } from "api/types.gen";
import type { State } from "store/state";
import { removeTag } from "api/sdk.gen";
import { toast } from "@andrewmclachlan/moo-ds";
import { buildTransactionsQueryKey, invalidateTransactionLists } from "./transactionKeys";

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutateAsync } = useMutation({
        mutationFn: (variables: { accountId: string, transactionId: string, tag: Tag }) =>
            removeTag({ path: { instrumentId: variables.accountId, id: variables.transactionId, tagId: variables.tag.id }, throwOnError: true }),
        onMutate: async (variables) => {

            const queryKey = buildTransactionsQueryKey(variables.accountId, filter, pageSize, currentPage, sortField, sortDirection);
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<PagedResult<Transaction>>(queryKey);
            if (!previous?.results) return { queryKey, previous: undefined };

            const next: PagedResult<Transaction> = {
                ...previous,
                results: previous.results.map(tr => {
                    if (tr.id !== variables.transactionId) return tr;

                    // Mirror backend Transaction.UpdateOrRemoveSplit: drop the tag from the split that holds
                    // it; if that empties the split and there's more than one, drop the split.
                    let splits = tr.splits ?? [];
                    const splitIndex = splits.findIndex(s => s.tags.some(t => t.id === variables.tag.id));
                    if (splitIndex !== -1) {
                        const split = splits[splitIndex];
                        const splitTags = split.tags.filter(t => t.id !== variables.tag.id);
                        splits = splitTags.length === 0 && splits.length > 1
                            ? splits.filter((_, i) => i !== splitIndex)
                            : splits.map((s, i) => i === splitIndex ? { ...s, tags: splitTags } : s);
                    }

                    return { ...tr, tags: tr.tags.filter(t => t.id !== variables.tag.id), splits };
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

    return {
        mutate: (variables: { accountId: string, transactionId: string, tag: Tag }) =>
            toast.promise(mutateAsync(variables), { pending: "Removing tag", success: "Tag removed", error: "Failed to remove tag" }),
    };
}
