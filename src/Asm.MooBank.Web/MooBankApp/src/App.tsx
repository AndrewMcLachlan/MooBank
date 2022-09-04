import "./App.scss";

import React from "react";
import { QueryClient, QueryClientProvider } from "react-query";
import { Provider as ReduxProvider } from "react-redux";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import { Layout } from "./layouts/Layout";
import * as Pages from "./pages";

import { AppStore } from "./store/configureStore";
import { AppProvider, HttpClientProvider, useMsal } from "./providers";

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
                                {isAuthenticated && <Routes>
                                    <Route path="/" element={<Pages.Home />} />
                                    <Route path="/accounts" element={<Pages.ManageAccounts />} />
                                    <Route path="/accounts/:id/manage" element={<Pages.ManageAccount />} />
                                    <Route path="/accounts/create" element={<Pages.CreateAccount />} />
                                    <Route path="/accounts/:id" element={<Pages.Transactions />} />
                                    <Route path="/accounts/:accountId/virtual/create" element={<Pages.CreateVirtualAccount />} />
                                    <Route path="/accounts/:accountId/virtual/:virtualid" element={<Pages.ManageVirtualAccount />} />
                                    <Route path="/accounts/:accountId/tag-rules" element={<Pages.TransactionTagRules />} />
                                    <Route path="/accounts/:id/import" element={<Pages.Import />} />
                                    <Route path="/settings" element={<Navigate to="/settings/tags" />} />
                                    <Route path="/settings/tags" element={<Pages.TransactionTags />} />
                                    <Route path="*" element={<Pages.Error404 />} />
                                </Routes>}
                            </Layout>
                        </BrowserRouter>
                    </ReduxProvider>
                </QueryClientProvider>
            </HttpClientProvider>
        </AppProvider>
    );
};

export default App;
