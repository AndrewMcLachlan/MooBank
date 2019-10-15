import { routerMiddleware, routerReducer } from "react-router-redux";
import { applyMiddleware, combineReducers, createStore } from "redux";
import thunk from "redux-thunk";
import { composeWithDevTools } from "redux-devtools-extension";

import * as Accounts from "./Accounts";
import * as App from "./App";
import * as Security from "./Security";
import { State } from "./state";
import * as Transactions from "./Transactions";
import * as TransactionTags from "./TransactionTags";
import * as TransactionTagRules from "./TransactionTagRules";

declare global {

    interface Window {
        devToolsExtension: any;
    }
}

export default function configureStore(history: any, initialState: State) {
    const reducers = {
        accounts: Accounts.reducer,
        transactions: Transactions.reducer,
        app: App.reducer,
        security: Security.reducer,
        transactionTags: TransactionTags.reducer,
        transactionTagRules: TransactionTagRules.reducer,
    };

    const middleware = [
        thunk,
        routerMiddleware(history),
    ];

    const rootReducer = combineReducers({
        ...reducers,
        routing: routerReducer,
    });

    return createStore(
        rootReducer,
        initialState,
        composeWithDevTools(applyMiddleware(...middleware)),
    );
}
