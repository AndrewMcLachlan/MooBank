import { useQueryClient } from "react-query";
import { useSelector } from "react-redux";

import * as Models from "../models";
import { TransactionTag } from "../models";
import { sortDirection, State, TransactionsFilter } from "../store/state";
import { useApiGet, useApiDelete, useApiDatalessPut } from "@andrewmclachlan/mooapp";

const transactionKey = "transactions";

interface TransactionTagVariables {
    accountId: string,
    transactionId: string,
    tag: TransactionTag,
}

export const useTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: sortDirection) => {

    const sortString = sortField && sortField !== null && sortField !== "" ? `sortField=${sortField}&sortDirection=${sortDirection}` : "";
    let filterString = filter.description ? `&filter=${filter.description}` : "";
        filterString += filter.start ? `&start=${filter.start}` : "";
        filterString += filter.end ? `&end=${filter.end}` : "";
        filterString += filter.tag ? `&tagid=${filter.tag}` : "";

    let queryString = sortString + filterString;
    queryString = queryString.startsWith("&") ? queryString.substr(1) : queryString;
    queryString = queryString.length > 0 && queryString[0] !== "?" ? `?${queryString}` : queryString;

    return useApiGet<Models.PagedResult<Models.Transaction>>([transactionKey, accountId, filter, pageSize, pageNumber, sortField, sortDirection], `api/accounts/${accountId}/transactions/${filter.filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}${queryString}`);
}

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiDatalessPut<Models.Transaction, TransactionTagVariables>((variables) => `api/accounts/${variables.accountId}/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {

            let transactions = queryClient.getQueryData<Models.PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection]);
            if (!transactions) return;

            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (!transaction) return;
            transaction.tags.push(variables.tag);

            queryClient.setQueryData<Models.PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection], transactions);
        },
    });
}

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiDelete<TransactionTagVariables>((variables) => `api/accounts/${variables.accountId}/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            
            let transactions = queryClient.getQueryData<Models.PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection]);
            if (!transactions) return;

            const transaction = transactions.results.find(tr => tr.id === variables.transactionId);
            if (!transaction) return;
            transaction.tags = transaction.tags.filter(t => t.id !== variables.tag.id);

            queryClient.setQueryData<Models.PagedResult<Models.Transaction>>([transactionKey, variables.accountId, filter, pageSize, currentPage, sortField, sortDirection], transactions);
        }
    });
}
