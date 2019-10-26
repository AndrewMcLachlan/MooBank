import "./App.scss";

import React from "react";
import { Provider } from "react-redux";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import { bindActionCreators, Dispatch } from "redux";

import { Layout } from "layouts/Layout";
import * as Pages from "./pages";

import { initialState as accountsInitialState } from "store/Accounts";
import configureStore from "store/configureStore";
import { State } from "store/state";
import { initialState as tagsInitialState, actionCreators as tagActionCreators } from "store/TransactionTags";
import { initialState as refDataInitialState, actionCreators as refDataActionCreators } from "store/ReferenceData";
import { initialState as rulesInitialState } from "store/TransactionTagRules";

const App: React.FC = () => {

    const initialState: State = {
        app: {
            appName: "MooBank", // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
            baseUrl: "/", //document.getElementsByTagName("base")[0].getAttribute("href"),
            skin: "moobank",// Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
        },
        accounts: accountsInitialState,
        transactionTags: tagsInitialState,
        transactionTagRules: rulesInitialState,
        referenceData: refDataInitialState,
    };

    const store = configureStore(window.history, initialState);

    const dispatch: Dispatch<any> =store.dispatch;

    bindActionCreators(tagActionCreators, dispatch);
    bindActionCreators(refDataActionCreators, dispatch);
    dispatch(tagActionCreators.requestTags());
    dispatch(refDataActionCreators.requestImporterTypes());

    return (
        <Provider store={store}>
            <BrowserRouter basename={initialState.app.baseUrl.replace(/^.*\/\/[^/]+/, "")}>
                <Layout>
                    <Switch>
                    <Route exact={true} path="/" component={Pages.Home} />
                    <Route exact path="/accounts" component={Pages.CreateAccount} />
                    <Route path="/accounts/create" component={Pages.CreateAccount} />
                    <Route exact path="/accounts/:id" component={Pages.Transactions} />
                    <Route path="/accounts/:id/tag-rules" component={Pages.TransactionTagRules} />
                    <Route path="/accounts/:id/import" component={Pages.Import} />
                    <Route exact path="/settings" component={Pages.TransactionTags} />
                    <Route path="/settings/tags" component={Pages.TransactionTags} />
                    </Switch>
                </Layout>
            </BrowserRouter>
        </Provider>
    );
};

export default App;
