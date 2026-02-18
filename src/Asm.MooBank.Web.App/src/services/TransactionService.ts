import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";
import { PagedResult, SortDirection } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import * as Models from "../models";
import { Tag } from "../models";
import { State, TransactionsFilter } from "../store/state";
import { accountsQueryKey } from "./AccountService";
import { toast } from "react-toastify";
import {
    getTransactionsOptions,
    getTransactionsQueryKey,
    getUntaggedTransactionsOptions,
    getUntaggedTransactionsQueryKey,
    searchTransactionsOptions,
    updateTransactionMutation,
    addTagMutation,
    removeTagMutation,
    createTransactionMutation,
} from "api/@tanstack/react-query.gen";
import {
    TransactionFilterType,
    SortDirection as GenSortDirection,
    TransactionType,
    CreateTransactionData,
} from "api/types.gen";

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
        ...getUntaggedTransactionsOptions({
            path: { instrumentId: accountId, pageSize, pageNumber, untagged: "untagged" },
            query: queryParams,
        }),
        select: (data) => data as unknown as PagedResult<Models.Transaction>,
        enabled: !!accountId && !!filter?.start && !!filter?.end && tagged,
    });

    const regularResult = useQuery({
        ...getTransactionsOptions({
            path: { instrumentId: accountId, pageSize, pageNumber },
            query: queryParams,
        }),
        select: (data) => data as unknown as PagedResult<Models.Transaction>,
        enabled: !!accountId && !!filter?.start && !!filter?.end && !tagged,
    });

    return tagged ? untaggedResult : regularResult;
}

export const useSearchTransactions = (transaction: Models.Transaction, searchType: Models.TransactionType) => {

    return useQuery({
        ...searchTransactionsOptions({
            path: { instrumentId: transaction.accountId },
            query: {
                Start: format(parseISO(transaction.transactionTime), 'yyyy-MM-dd'),
                TransactionType: searchType as TransactionType,
                TagIds: transaction.tags.map(t => t.id),
            },
        }),
        select: (data) => data as unknown as Models.Transaction[],
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

            const queryKey = buildTransactionsQueryKey(variables.path!.instrumentId, filter, pageSize, currentPage, sortField, sortDirection);
            const transactions = { ...queryClient.getQueryData<PagedResult<Models.Transaction>>(queryKey) };
            if (!transactions?.results) return;

            const transaction = transactions.results.find(tr => tr.id === variables.path!.id);
            if (!transaction) return;

            const body = variables.body as Models.TransactionUpdate;
            transaction.notes = body.notes;
            transaction.splits = body.splits;
            transaction.excludeFromReporting = body.excludeFromReporting;
            transaction.tags = body.splits.flatMap(s => s.tags);

            queryClient.setQueryData<PagedResult<Models.Transaction>>(queryKey, transactions);

        },
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTransactionsQueryKey({ path: { instrumentId: "", pageSize: 0, pageNumber: 0 } } as any) });
        }
    });

    return {
        mutateAsync: (accountId: string, transactionId: string, transaction: Models.TransactionUpdate) =>
            toast.promise(mutateAsync({ body: transaction as any, path: { instrumentId: accountId, id: transactionId } }), { pending: "Updating transaction", success: "Transaction updated", error: "Failed to update transaction" }),
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
        const transactions = { ...queryClient.getQueryData<PagedResult<Models.Transaction>>(queryKey) };
        if (transactions?.results) {
            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (transaction) {
                transaction.tags.push(variables.tag);
                queryClient.setQueryData<PagedResult<Models.Transaction>>(queryKey, transactions);
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
        const transactions = { ...queryClient.getQueryData<PagedResult<Models.Transaction>>(queryKey) };
        if (transactions?.results) {
            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (transaction) {
                transaction.tags = transaction.tags.filter(t => t.id !== variables.tag.id);
                queryClient.setQueryData<PagedResult<Models.Transaction>>(queryKey, transactions);
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
        mutateAsync: (accountId: string, transaction: Models.CreateTransaction) =>
            toast.promise(mutateAsync({ body: transaction as unknown as CreateTransactionData["body"], path: { instrumentId: accountId } }), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" }),
        ...rest,
    };
}
