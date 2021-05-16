import "./App.scss";

import React from "react";
import { QueryClient, QueryClientProvider } from "react-query";
import { Provider as ReduxProvider } from "react-redux";
import { BrowserRouter, Redirect, Route, Switch } from "react-router-dom";

import { Layout } from "./layouts/Layout";
import * as Pages from "./pages";

import { AppStore } from "./store/configureStore";
import { AppProvider, HttpClientProvider, useMsal } from "./components";

const App: React.FC = () => {

    const baseUrl = "/"; //document.getElementsByTagName("base")[0].getAttribute("href");

    const { isAuthenticated, loading, login } = useMsal();

    if (!isAuthenticated && !loading) {
        login("loginRedirect");
    }

    const queryClient = new QueryClient();

    return (
        <AppProvider appName="MooBank" // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
            baseUrl={baseUrl} //document.getElementsByTagName("base")[0].getAttribute("href"),
            skin="moobank" // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
        >
            <HttpClientProvider baseUrl={baseUrl}>
                <QueryClientProvider client={queryClient}>
                    <ReduxProvider store={AppStore}>
                        <BrowserRouter basename={baseUrl.replace(/^.*\/\/[^/]+/, "")}>
                            <Layout>
                                {isAuthenticated && <Switch>
                                    <Route exact={true} path="/" component={Pages.Home} />
                                    <Route exact path="/accounts" component={Pages.ManageAccounts} />
                                    <Route exact path="/accounts/:id/manage" component={Pages.ManageAccount} />
                                    <Route path="/accounts/create" component={Pages.CreateAccount} />
                                    <Route exact path="/accounts/:id" component={Pages.Transactions} />
                                    <Route path="/accounts/:accountId/virtual/create" component={Pages.CreateVirtualAccount} />
                                    <Route path="/accounts/:accountId/virtual/:virtualid" component={Pages.ManageVirtualAccount} />
                                    <Route path="/accounts/:accountId/tag-rules" component={Pages.TransactionTagRules} />
                                    <Route path="/accounts/:id/import" component={Pages.Import} />
                                    <Redirect exact path="/settings" to="/settings/tags" />
                                    <Route path="/settings/tags" component={Pages.TransactionTags} />
                                    <Route component={Pages.Error404} />
                                </Switch>}
                            </Layout>
                        </BrowserRouter>
                    </ReduxProvider>
                </QueryClientProvider>
            </HttpClientProvider>
        </AppProvider>
    );
};

export default App;
