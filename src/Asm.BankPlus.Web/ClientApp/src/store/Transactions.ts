import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { sortDirection, Transactions } from "./state";

const initialState: Transactions = {
    currentPage: 1,
    pageSize: 50,
    filterTagged: false,
    sortField: "",
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

    setTransactionListFilter: (state: Transactions, action: PayloadAction<boolean>) => {
        return {
            ...state,
            filterTagged: action.payload,
        };
    },

    setSort: (state: Transactions, action: PayloadAction<[string, sortDirection]>) => {
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

    setSortDirection: (state: Transactions, action: PayloadAction<sortDirection>) => {
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


