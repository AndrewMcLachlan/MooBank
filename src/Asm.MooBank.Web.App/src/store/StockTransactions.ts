import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { SortDirection } from "@andrewmclachlan/moo-ds";
import { Transactions, TransactionsFilter } from "./state";

const initialState: Transactions = {
    currentPage: 1,
    pageSize: 50,
    filter: {
        filterTagged: false,
        tags: null,
        transactionType: "",
    },
    sortField: "TransactionDate",
    sortDirection: "Descending",
};

export const reducers = {

    setCurrentPage: (state: Transactions, action: PayloadAction<number>) => {
        return {
            ...state,
            currentPage: action.payload,
        };
    },

    setPageSize: (state: Transactions, action: PayloadAction<number>) => {
        return {
            ...state,
            pageSize: action.payload,
        };
    },

    setTransactionListFilter: (state: Transactions, action: PayloadAction<TransactionsFilter>): Transactions => {

        const newFilter: TransactionsFilter = {
            description: action.payload.description ?? state.filter.description,
            transactionType: "",
            start: action.payload.start ?? state.filter.start,
            end: action.payload.end ?? state.filter.end,
        };

        return {
            ...state,
            filter: newFilter,
        };
    },

    setSort: (state: Transactions, action: PayloadAction<[string, SortDirection]>) => {
        return {
            ...state,
            sortField: action.payload[0],
            sortDirection: action.payload[1],
        };
    },

    setSortField: (state: Transactions, action: PayloadAction<string>) => {
        return {
            ...state,
            sortField: action.payload,
        };
    },

    setSortDirection: (state: Transactions, action: PayloadAction<SortDirection>) => {
        return {
            ...state,
            sortDirection: action.payload,
        };
    },
};

export const StockTransactionsSlice = createSlice({
    name: "StockTransactions",
    initialState: initialState,
    reducers: reducers
});


