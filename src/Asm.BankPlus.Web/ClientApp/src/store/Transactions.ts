import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { Transactions } from "./state";

export const SetTransactionListFilter = "SetTransactionListFilter";
export const SetCurrentPage = "SetCurrentPage";

const initialState: Transactions = {
    currentPage: 1,
    pageSize: 50,
    filterTagged: false,
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
};

export const TransactionsSlice = createSlice({
    name: "Transactions",
    initialState: initialState,
    reducers: reducers
});


