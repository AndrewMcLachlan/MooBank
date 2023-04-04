import "./App.scss";

import React from "react";
import { Link, Navigate, Route, Routes } from "react-router-dom";

import * as Pages from "./pages";
import { Layout } from "@andrewmclachlan/mooapp";
import { useIsAuthenticated } from "@azure/msal-react";

const App: React.FC = () => {

    const isAuthenticated = useIsAuthenticated();

    if (!isAuthenticated) return null;

    return (
        <Layout size="small">
            <Layout.Header AppName="MooBank" Menu={[
                    <Link to="/accounts">Manage Accounts</Link>,
                    <Link to="/account-groups">Account Groups</Link>,
                    <Link to="/settings">Settings</Link>
            ]} />
            <Layout.Article>
            <Routes>
                <Route path="/" element={<Pages.Home />} />
                <Route path="/accounts" element={<Pages.ManageAccounts />} />
                <Route path="/accounts/:id/manage" element={<Pages.ManageAccount />} />
                <Route path="/accounts/create" element={<Pages.CreateAccount />} />
                <Route path="/accounts/:id" element={<Pages.Transactions />} />
                <Route path="/accounts/:accountId/virtual/create" element={<Pages.CreateVirtualAccount />} />
                <Route path="/accounts/:accountId/virtual/:virtualid" element={<Pages.ManageVirtualAccount />} />
                <Route path="/accounts/:accountId/tag-rules" element={<Pages.TransactionTagRules />} />
                <Route path="/accounts/:id/import" element={<Pages.Import />} />
                <Route path="/accounts/:id/reports" element={<Pages.InOut />} />
                <Route path="/settings" element={<Navigate to="/settings/tags" />} />
                <Route path="/settings/tags" element={<Pages.TransactionTags />} />
                <Route path="*" element={<Pages.Error404 />} />
                <Route path="/account-groups" element={<Pages.ManageAccountGroups />} />
                <Route path="/account-groups/:id/manage" element={<Pages.ManageAccountGroup />} />
                <Route path="/account-groups/create" element={<Pages.CreateAccountGroup />} />
            </Routes>
            </Layout.Article>
            <Layout.Footer copyrightYear={2013} />
        </Layout>
    );
};

export default App;
