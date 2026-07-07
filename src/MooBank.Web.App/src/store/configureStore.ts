import { combineReducers, configureStore } from "@reduxjs/toolkit";

import { TransactionsSlice } from "./Transactions";
import { StockTransactionsSlice } from "./StockTransactions";

const rootReducer = combineReducers(
    {
        stockTransactions: StockTransactionsSlice.reducer,
        transactions: TransactionsSlice.reducer,
    });

export const AppStore: any = configureStore({
    reducer: rootReducer
});
