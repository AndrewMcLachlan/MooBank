import "./App.scss";
import "font-awesome/css/font-awesome.min.css";

import React from "react";
import { Provider } from "react-redux";

import { BrowserRouter, Route } from "react-router-dom";
import { Layout } from "layouts/Layout";
import * as Pages from "./pages";
import configureStore from "store/configureStore";
import { State } from "store/state";
import { initialState as accountsInitialState } from "store/Accounts";
import { initialState as tagsInitialState } from "store/TransactionTags";
import { bindActionCreators, Dispatch } from "redux";
import { actionCreators } from "store/TransactionTags";

const App: React.FC = () => {

    const initialState: State = {
        app: {
            appName: "MooBank", // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
            baseUrl: "/", //document.getElementsByTagName("base")[0].getAttribute("href"),
            skin: "moobank",// Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
        },
        accounts: accountsInitialState,
        transactionTags: tagsInitialState,
    };

    const store = configureStore(window.history, initialState);

    const dispatch: Dispatch<any> =store.dispatch;

    bindActionCreators(actionCreators, dispatch);
    dispatch(actionCreators.requestTags());

    return (
        <Provider store={store}>
            <BrowserRouter basename={initialState.app.baseUrl.replace(/^.*\/\/[^/]+/, "")}>
                <Layout>
                    <Route exact={true} path="/" component={Pages.Home} />
                    <Route path="/accounts" component={Pages.ManageAccounts} />
                    <Route exact path="/accounts/:id" component={Pages.Transactions} />
                    <Route path="/accounts/:id/import" component={Pages.Import} />
                    <Route exact path="/settings" component={Pages.Settings} />
                    <Route path="/settings/tags" component={Pages.TransactionTags} />
                </Layout>
            </BrowserRouter>
        </Provider>
    );
};

export default App;
