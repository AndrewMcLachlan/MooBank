import { UseQueryResult, useQueryClient } from "@tanstack/react-query";

import * as Models from "../models";
import { TransactionsFilter } from "../store/state";
import { PagedResult, SortDirection, useApiPagedGet, useApiPost } from "@andrewmclachlan/mooapp";
import { toast } from "react-toastify";

const transactionKey = "stock-transactions";

export const useStockTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection): UseQueryResult<PagedResult<Models.StockTransaction>> => {

    const sortString = sortField && sortField !== null && sortField !== "" ? `sortField=${sortField}&sortDirection=${sortDirection}` : "";
    let filterString = filter.description ? `&filter=${filter.description}` : "";
        filterString += filter.start ? `&start=${filter.start}` : "";
        filterString += filter.end ? `&end=${filter.end}` : "";

    let queryString = sortString + filterString;
    queryString = queryString.startsWith("&") ? queryString.substring(1) : queryString;
    queryString = queryString.length > 0 && queryString[0] !== "?" ? `?${queryString}` : queryString;

    return useApiPagedGet<PagedResult<Models.StockTransaction>>([transactionKey, accountId, filter, pageSize, pageNumber, sortField, sortDirection], `api/stocks/${accountId}/transactions/${filter.filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}${queryString}`);
}

export const useCreateStockTransaction = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useApiPost<Models.StockTransaction, { accountId: string }, Models.CreateStockTransaction>((variables) => `api/stocks/${variables.accountId}/transactions`, {
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: [transactionKey]});
        }
    });

    const create = (accountId:string, transaction: Models.CreateStockTransaction) => {
        toast.promise(mutateAsync([{accountId}, transaction]), { pending: "Creating transaction", success: "Transaction created", error: "Failed to create transaction" });
    };

    return create;
}
