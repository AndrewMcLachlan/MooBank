import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { SortDirection } from "@andrewmclachlan/mooapp";
import { Transactions, TransactionsFilter } from "./state";

const initialState: Transactions = {
    currentPage: 1,
    pageSize: 50,
    filter: {
        filterTagged: false,
        tags: null,
        transactionType: "",
    },
    sortField: "TransactionTime",
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
            filterTagged: action.payload.filterTagged ?? state.filter.filterTagged,
            tags: action.payload.tags === undefined ? state.filter.tags : action.payload.tags,
            transactionType: action.payload.transactionType ?? state.filter.transactionType,
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

export const TransactionsSlice = createSlice({
    name: "Transactions",
    initialState: initialState,
    reducers: reducers
});


