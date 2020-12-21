import "./App.scss";

import React, { useEffect } from "react";
import { QueryClient, QueryClientProvider } from "react-query";
import { Provider as ReduxProvider } from "react-redux";
import { BrowserRouter, Route, Switch } from "react-router-dom";

import { Layout } from "./layouts/Layout";
import * as Pages from "./pages";

import { initialState as accountsInitialState } from "./store/Accounts";
import configureStore from "./store/configureStore";
import { State } from "./store/state";
import { initialState as appInitialState } from "./store/App";
import { apiRequest, AppProvider, useMsal } from "./components";
import { createHttpClient, httpClient } from "./services/HttpClient";

const App: React.FC = () => {

    const baseUrl = "/"; //document.getElementsByTagName("base")[0].getAttribute("href");

    const initialState: State = {
        app: appInitialState,
        accounts: accountsInitialState,
    };

    const { isAuthenticated, getToken } = useMsal();

    useEffect(() => {
        createHttpClient(baseUrl);
        httpClient.interceptors.request.use(async (request) => {
            request.headers = {
                "Authorization": `Bearer ${await getToken(apiRequest, "loginRedirect")}`
            };
            return request;
        })
    }, []);

    if (isAuthenticated) {
        getToken(apiRequest, "loginRedirect").then((r) => window.token = r);
    }

    const queryClient = new QueryClient();

    const store = configureStore(window.history, initialState);

    return (
        <AppProvider appName="MooBank" // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
            baseUrl={baseUrl} //document.getElementsByTagName("base")[0].getAttribute("href"),
            skin="moobank" // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
        >
            <QueryClientProvider client={queryClient}>
                <ReduxProvider store={store}>
                    <BrowserRouter basename={baseUrl.replace(/^.*\/\/[^/]+/, "")}>
                        <Layout>
                            {isAuthenticated && <Switch>
                                <Route exact={true} path="/" component={Pages.Home} />
                                <Route exact path="/accounts" component={Pages.CreateAccount} />
                                <Route path="/accounts/create" component={Pages.CreateAccount} />
                                <Route exact path="/accounts/:id" component={Pages.Transactions} />
                                <Route path="/accounts/:accountId/tag-rules" component={Pages.TransactionTagRules} />
                                <Route path="/accounts/:id/import" component={Pages.Import} />
                                <Route exact path="/settings" component={Pages.TransactionTags} />
                                <Route path="/settings/tags" component={Pages.TransactionTags} />
                            </Switch>}
                        </Layout>
                    </BrowserRouter>
                </ ReduxProvider>
            </QueryClientProvider>
        </AppProvider>
    );
};

export default App;
