import "./App.scss";

import React from "react";
import { Provider } from "react-redux";

import { useSecurityService } from "services/SecurityService";

import { BrowserRouter, Route } from "react-router-dom";
import { Layout } from "layouts/Layout";
import * as Pages from "./pages";
import configureStore from "store/configureStore";
import { State } from "store/state";

const App: React.FC = () => {

  const initialState: State = {
    app: {
      appName: "MooBank", // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
      baseUrl: "/", //document.getElementsByTagName("base")[0].getAttribute("href"),
      skin: "moobank",// Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
    },
  };

  const securityService = useSecurityService();

  const store = configureStore(window.history, initialState);

  return (
    <Provider store={store}>
      <BrowserRouter basename={initialState.app.baseUrl.replace(/^.*\/\/[^/]+/, "")}>
        <Layout>
          <Route exact={true} path="/" component={Pages.Home} />
          <Route path="/accounts" component={Pages.ManageAccounts} />
          <Route exact path="/settings" component={Pages.Settings} />
          <Route path="/settings/transaction-categories" component={Pages.TransactionCategories} />
        </Layout>
      </BrowserRouter>
    </Provider>
  );
};

export default App;
