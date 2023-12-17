import { UseQueryResult, useQueryClient } from "@tanstack/react-query";
import { useSelector } from "react-redux";

import * as Models from "../models";
import { Transaction, Tag } from "../models";
import { State, TransactionsFilter } from "../store/state";
import { SortDirection, useApiGet, useApiPagedGet, useApiDelete, useApiDatalessPut, useApiPatch, useApiDatalessPost } from "@andrewmclachlan/mooapp";
import format from "date-fns/format";
import parseISO from "date-fns/parseISO";

const transactionKey = "stock-transactions";

interface TransactionVariables {
    accountId: string,
    transactionId: string,
}

export const useStockTransactions = (accountId: string, filter: TransactionsFilter, pageSize: number, pageNumber: number, sortField: string, sortDirection: SortDirection): UseQueryResult<Models.PagedResult<Models.StockTransaction>> => {

    const sortString = sortField && sortField !== null && sortField !== "" ? `sortField=${sortField}&sortDirection=${sortDirection}` : "";
    let filterString = filter.description ? `&filter=${filter.description}` : "";
        filterString += filter.start ? `&start=${filter.start}` : "";
        filterString += filter.end ? `&end=${filter.end}` : "";

    let queryString = sortString + filterString;
    queryString = queryString.startsWith("&") ? queryString.substring(1) : queryString;
    queryString = queryString.length > 0 && queryString[0] !== "?" ? `?${queryString}` : queryString;

    return useApiPagedGet<Models.PagedResult<Models.StockTransaction>>([transactionKey, accountId, filter, pageSize, pageNumber, sortField, sortDirection], `api/stock/${accountId}/transactions/${filter.filterTagged ? "untagged/" : ""}${pageSize}/${pageNumber}${queryString}`);
}

