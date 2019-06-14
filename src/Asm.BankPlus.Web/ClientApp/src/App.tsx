import React from "react";
import { Route } from "react-router-dom";

import Layout from "./components/Layout";
import Home from "./pages/Home";
import ManageAccounts from "./pages/ManageAccounts";
import Settings from "./pages/Settings";

export default () => (
    <Layout>
        <Route exact={true} path="/" component={Home} />
        <Route path="/accounts" component={ManageAccounts} />
        <Route path="/settings" component={Settings} />
    </Layout>
);
