import { useQueryClient } from "react-query";
import { useSelector } from "react-redux";

import * as Models from "../models";
import { TransactionTag } from "../models";
import { sortDirection, State } from "../store/state";
import { useApiGet, useApiDelete, useApiDatalessPut } from "./api";

const transactionKey = "transactions";

interface TransactionTagVariables {
    accountId: string,
    transactionId: string,
    tag: TransactionTag,
}

export const useTransactions = (accountId: string, filterTagged: boolean, pageSize: number, pageNumber: number, sortField: string, sortDirection: sortDirection) => {

    const sortString = sortField && sortField !== null && sortField !== "" ? `?sortField=${sortField}&sortDirection=${sortDirection}` : "";

    return useApiGet<Models.Transactions>([transactionKey, accountId, filterTagged, pageSize, pageNumber, sortField, sortDirection], `api/accounts/${accountId}/transactions/${filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}${sortString}`);
}

export const useAddTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filterTagged, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiDatalessPut<Models.Transaction, TransactionTagVariables>((variables) => `api/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            queryClient.setQueryData<Models.Transactions>([transactionKey, variables.accountId, filterTagged, pageSize, currentPage, sortField, sortDirection], (t) => {
                const data = t.transactions.find(t => t.id === variables.transactionId);
                data.tags.push(variables.tag);
                return t;
            });
        },
    });
}

export const useRemoveTransactionTag = () => {

    const queryClient = useQueryClient();

    const { currentPage, pageSize, filterTagged, sortField, sortDirection } = useSelector((state: State) => state.transactions);

    return useApiDelete<TransactionTagVariables>((variables) => `api/transactions/${variables.transactionId}/tag/${variables.tag.id}`, {
        onMutate: (variables) => {
            
            queryClient.setQueryData<Models.Transactions>([transactionKey, variables.accountId, filterTagged, pageSize, currentPage, sortField, sortDirection], (t => {
                const transaction = t.transactions.find(tr => tr.id === variables.transactionId);
                transaction.tags = transaction.tags.filter(t => t.id !== variables.tag.id);
                return t;
            }));
        }
    });
}
