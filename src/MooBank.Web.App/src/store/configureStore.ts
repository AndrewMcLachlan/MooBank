import { combineReducers, configureStore } from "@reduxjs/toolkit";
//import { composeWithDevTools } from "redux-devtools-extension";

import { AppSlice } from "./App";
import { TransactionsSlice } from "./Transactions";
import { StockTransactionsSlice } from "./StockTransactions";

const rootReducer = combineReducers(
    {
        app: AppSlice.reducer,
        stockTransactions: StockTransactionsSlice.reducer,
        transactions: TransactionsSlice.reducer,
    });

//const enhancer = composeWithDevTools({});

export const AppStore: any = configureStore({
    reducer: rootReducer
});