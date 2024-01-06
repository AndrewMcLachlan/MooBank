import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";

import * as Models from "../models";
import { Transaction, Tag } from "../models";
import { State, TransactionsFilter } from "../store/state";
import { PagedResult, SortDirection, useApiGet, useApiPagedGet, useApiDelete, useApiPutEmpty, useApiPatch, useApiPost } from "@andrewmclachlan/mooapp";
import {format} from "date-fns/format";
import {parseISO} from "date-fns/parseISO";

const transactionKey = "transactions";

interface TransactionVariables {
    accountId: string,
    transactionId: string,
}

interface TransactionTagVariables extends TransactionVariables {
     tag: Tag,
}

export const useTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection): UseQueryResult<PagedResult<Models.Transaction>> => {

    const sortString = sortField && sortField !== null && sortField !== "" ? `sortField=${sortField}&sortDirection=${sortDirection}` : "";
    let filterString = filter.description ? `&filter=${filter.description}` : "";
        filterString += filter.start ? `&start=${filter.start}` : "";
        filterString += filter.end ? `&end=${filter.end}` : "";
        filter.tags && filter.tags.forEach(t => filterString += `&tagids=${t}`);

    let queryString = sortString + filterString;
    queryString = queryString.startsWith("&") ? queryString.substring(1) : queryString;
    queryString = queryString.length > 0 && queryString[0] !== "?" ? `?${queryString}` : queryString;

    return useApiPagedGet<PagedResult<Models.Transaction>>([transactionKey, accountId, filter, pageSize, pageNumber, sortField, sortDirection],
        `api/accounts/${accountId}/transactions/${filter.filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}${queryString}`, {
        enabled: !!accountId && !!filter?.start && !!filter?.end,
    });
}

export const useSearchTransactions = (transaction: Models.Transaction, searchType: Models.TransactionType) => {

    let queryString = `?start=${format(parseISO(transaction.transactionTime), 'yyyy-MM-dd')}&transactionType=${searchType}&`;

    queryString += transaction.tags.map(t => `tagIds=${t.id}`).join(`&`);

    return useApiGet<Models.Transaction[]>([transactionKey, transaction.id], `api/accounts/${transaction.accountId}/transactions${queryString}`)
}

export const useInvalidateSearch = (transactionId: string) => {

    const queryClient = useQueryClient();

    return () => queryClient.invalidateQueries({ queryKey: [transactionKey, transactionId]});
}

export const useUpdateTransaction = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiPatch<Transaction, TransactionVariables, Models.TransactionUpdate>((variables) => `api/accounts/${variables.accountId}/transactions/${variables.transactionId}`, {
        onMutate: ([variables, data]) => {

            const transactions = {...queryClient.getQueryData<PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection])};
            if (!transactions) return;

            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (!transaction) return;

            transaction.notes = data.notes;
            transaction.splits = data.splits;
            transaction.excludeFromReporting = data.excludeFromReporting;
            transaction.tags = data.splits.flatMap(s => s.tags);

            queryClient.setQueryData<PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection], transactions);

        },
        onError: (_error, [variables]) => {
            queryClient.invalidateQueries({ queryKey: [transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection]});
        }
    });
}

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiPutEmpty<Models.Transaction, TransactionTagVariables>((variables) => `api/accounts/${variables.accountId}/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {

            const transactions = {...queryClient.getQueryData<PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection])};
            if (!transactions) return;

            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (!transaction) return;
            transaction.tags.push(variables.tag);

            queryClient.setQueryData<PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection], transactions);

        },
        onError: (_error, variables) => {
            queryClient.invalidateQueries({ queryKey: [transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection]});
        }
    });
}

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiDelete<TransactionTagVariables>((variables) => `api/accounts/${variables.accountId}/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            
            const transactions = {...queryClient.getQueryData<PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection])};
            if (!transactions) return;

            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (!transaction) return;
            transaction.tags = transaction.tags.filter(t => t.id !== variables.tag.id);

            queryClient.setQueryData<PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection], transactions);
        },
        onError: (_error, variables) => {
            queryClient.invalidateQueries({ queryKey: [transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection]});
        }
    });
}

export const useCreateTransaction = () => {

    const queryClient = useQueryClient();

    const res = useApiPost<Transaction, { accountId: string }, Models.CreateTransaction>((variables) => `api/accounts/${variables.accountId}/transactions`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [transactionKey]});
        }
    });

    const { mutate } = res;

    const create = (accountId:string, transaction: Models.CreateTransaction) => {
        mutate([{accountId}, transaction]);
    };

    return create;
}
