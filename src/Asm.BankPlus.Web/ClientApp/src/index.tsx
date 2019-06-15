import React from "react";
import ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { BrowserRouter } from "react-router-dom";

import App from "./App";
import registerServiceWorker from "./registerServiceWorker";
import configureStore from "./store/configureStore";
import { State } from "./store/state";
import { UserAgentApplication } from "msal";
import * as securityConfig from "./securityConfiguration";

// Get the application-wide store instance, prepopulating with state from the server where available.
const initialState = {
    app: {
        appName: Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
        baseUrl: document.getElementsByTagName("base")[0].getAttribute("href"),
        skin: Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
    },
    security: {
        msal: new UserAgentApplication(securityConfig.msalConfig),
    },
};

const store = configureStore(history, initialState);

const rootElement = document.getElementById("root");

ReactDOM.render(
    (
        <Provider store={store}>
            <BrowserRouter basename={initialState.app.baseUrl.replace(/^.*\/\/[^\/]+/, "")}>
                <App />
            </BrowserRouter>
        </Provider>
    ),
    rootElement,
);

registerServiceWorker();
