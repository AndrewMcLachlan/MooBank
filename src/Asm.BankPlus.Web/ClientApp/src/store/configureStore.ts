import { combineReducers, createStore } from "@reduxjs/toolkit";
import { composeWithDevTools } from "redux-devtools-extension";

import { AppSlice } from "./App";
import { TransactionsSlice } from "./Transactions";

const rootReducer = combineReducers(
    {
        app: AppSlice.reducer,
        transactions: TransactionsSlice.reducer,
    });

const enhancer = composeWithDevTools();

export const AppStore = createStore(rootReducer, enhancer);