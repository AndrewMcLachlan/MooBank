import { routerMiddleware, routerReducer } from "react-router-redux";
import { applyMiddleware, combineReducers, createStore } from "redux";
import thunk from "redux-thunk";
import { composeWithDevTools } from "redux-devtools-extension";

import * as App from "./App";
import { State } from "./state";
import * as Transactions from "./Transactions";

declare global {

    interface Window {
        devToolsExtension: any;
    }
}

export default function configureStore(history: any, initialState: State) {
    const reducers = {
        transactions: Transactions.reducer,
        app: App.reducer,
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
