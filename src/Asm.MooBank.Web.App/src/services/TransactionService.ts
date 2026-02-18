import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import { PagedResult, SortDirection } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import type { Transaction, Tag, TransactionType, TransactionFilterType, SortDirection as GenSortDirection } from "api/types.gen";
import type { TransactionUpdate, CreateTransaction } from "helpers/transactions";
import { State, TransactionsFilter } from "../store/state";
import { accountsQueryKey } from "./AccountService";
import { toast } from "react-toastify";
import {
    getTransactionsQueryKey,
    getUntaggedTransactionsQueryKey,
    searchTransactionsOptions,
    updateTransactionMutation,
    addTagMutation,
    removeTagMutation,
    createTransactionMutation,
} from "api/@tanstack/react-query.gen";
import { getTransactions, getUntaggedTransactions } from "api/sdk.gen";

const buildTransactionsQueryKey = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {
    if (filter.filterTagged) {
        return getUntaggedTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
            query: {
                Filter: filter.description || undefined,
                Start: filter.start || undefined,
                End: filter.end || undefined,
                TagIds: filter.tags,
                SortField: sortField || undefined,
                TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
                SortDirection: (sortDirection || "Descending") as GenSortDirection,
                ExcludeNetZero: filter.filterNetZero || undefined,
            },
        });
    }
    return getTransactionsQueryKey({
        path: { instrumentId: accountId, pageSize, pageNumber },
        query: {
            Filter: filter.description || undefined,
            Start: filter.start || undefined,
            End: filter.end || undefined,
            TagIds: filter.tags,
            SortField: sortField || undefined,
            TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
            SortDirection: (sortDirection || "Descending") as GenSortDirection,
            ExcludeNetZero: filter.filterNetZero || undefined,
        },
    });
};

export const useTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection) => {

    const queryParams = {
        Filter: filter.description || undefined,
        Start: filter.start || undefined,
        End: filter.end || undefined,
        TagIds: filter.tags,
        SortField: sortField || undefined,
        TransactionType: (filter.transactionType || undefined) as TransactionFilterType | undefined,
        SortDirection: (sortDirection || "Descending") as GenSortDirection,
        ExcludeNetZero: filter.filterNetZero || undefined,
    };

    const tagged = filter.filterTagged;

    const untaggedResult = useQuery({
        queryKey: getUntaggedTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
            query: queryParams,
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getUntaggedTransactions({
                path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
                query: queryParams,
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Transaction>;
        },
        enabled: !!accountId && !!filter?.start && !!filter?.end && tagged,
    });

    const regularResult = useQuery({
        queryKey: getTransactionsQueryKey({
            path: { instrumentId: accountId, pageSize, pageNumber },
            query: queryParams,
        }),
        queryFn: async ({ signal }) => {
            const { data, headers } = await getTransactions({
                path: { instrumentId: accountId, pageSize, pageNumber },
                query: queryParams,
                signal,
                throwOnError: true,
            });
            return { results: data, total: Number(headers['x-total-count'] ?? 0) } as PagedResult<Transaction>;
        },
        enabled: !!accountId && !!filter?.start && !!filter?.end && !tagged,
    });

    return tagged ? untaggedResult : regularResult;
}

export const useSearchTransactions = (transaction: Transaction, searchType: TransactionType) => {

    return useQuery({
        ...searchTransactionsOptions({
            path: { instrumentId: transaction.accountId },
            query: {
                Start: format(parseISO(transaction.transactionTime), 'yyyy-MM-dd'),
                TransactionType: searchType,
                TagIds: transaction.tags.map(t => t.id),
            },
        }),
    });
}

export const useInvalidateSearch = (transactionId: string) => {

    const queryClient = useQueryClient();

    return () => queryClient.invalidateQueries({ queryKey: searchTransactionsOptions({ path: { instrumentId: transactionId } } as any).queryKey });
}

export const useUpdateTransaction = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const { mutateAsync, ...rest } = useMutation({
        ...updateTransactionMutation(),
        onMutate: (variables) => {

            const queryKey = buildTransactionsQueryKey((variables as any).path!.instrumentId, filter, pageSize, currentPage, sortField, sortDirection);
            const transactions = { ...queryClient.getQueryData<PagedResult<Transaction>>(queryKey) };
            if (!transactions?.results) return;

            const transaction = transactions.results.find(tr => tr.id === (variables as any).path!.id);
            if (!transaction) return;

            const body = variables.body as TransactionUpdate;
            transaction.notes = body.notes;
            transaction.splits = body.splits;
            transaction.excludeFromReporting = body.excludeFromReporting;
            transaction.tags = body.splits.flatMap(s => s.tags);

            queryClient.setQueryData<PagedResult<Transaction>>(queryKey, transactions);

        },
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
        }
    });

    return {
        mutateAsync: (accountId: string, transactionId: string, transaction: TransactionUpdate) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId, id: transactionId } } as any), { pending: "Updating transaction", success: "Transaction updated", error: "Failed to update transaction" }),
        ...rest,
    };
};

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
                queryClient.setQueryData<PagedResult<Transaction>>(queryKey, transactions);
            }
        }

        rawMutate({ path: { instrumentId: variables.accountId, id: variables.transactionId, tagId: variables.tag.id } });
    };

    return { mutate };
}

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

export const useCreateTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createTransactionMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
            queryClient.refetchQueries({ queryKey: accountsQueryKey() });
        }
    });

    return {
        mutateAsync: (accountId: string, transaction: CreateTransaction) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId } } as any), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" }),
        ...rest,
    };
}
