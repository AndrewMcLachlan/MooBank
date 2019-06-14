import { routerMiddleware, routerReducer } from "react-router-redux";
import { applyMiddleware, combineReducers, compose, createStore } from "redux";
import thunk from "redux-thunk";

import * as Accounts from "./Accounts";
import * as App from "./App";
import * as Security from "./Security";

declare global {

    interface Window {
        __REDUX_DEVTOOLS_EXTENSION_COMPOSE__: any;
        devToolsExtension: any;
    }
}

export default function configureStore(history, initialState) {
    const reducers = {
        accounts: Accounts.reducer,
        app: App.reducer,
        security: Security.reducer,
    };

    const middleware = [
        thunk,
        routerMiddleware(history),
    ];

    // In development, use the browser"s Redux dev tools extension if installed
    const enhancers = [];
    const isDevelopment = process.env.NODE_ENV === "development";
    if (isDevelopment && typeof window !== "undefined" && window.devToolsExtension) {
        enhancers.push(window.devToolsExtension());
    }

    const rootReducer = combineReducers({
        ...reducers,
        routing: routerReducer,
    });

    return createStore(
        rootReducer,
        initialState,
        compose(applyMiddleware(...middleware), ...enhancers),
    );
}
