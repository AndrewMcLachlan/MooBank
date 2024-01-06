import "./App.scss";
import * as Icons from "./assets";

import React from "react";
import { Link, Route, Routes } from "react-router-dom";

import * as Pages from "./pages";
import { Alerts, ErrorBoundary, Layout } from "@andrewmclachlan/mooapp";
import { useIsAuthenticated } from "@azure/msal-react";
import { useHasRole } from "hooks";

const App: React.FC = () => {

    const isAuthenticated = useIsAuthenticated();

    const hasRole = useHasRole();

    if (!isAuthenticated) return null;

    const menu = hasRole("Admin") ?
        [(<Link key="settings" to="/settings"><Icons.Cog /></Link>)] :
        [];

    return (
        <Layout size="small">
            <Alerts />
            <Layout.Header AppName="" Menu={menu} />
            <Layout.Sidebar navItems={[
                {
                    text: "Dashboard",
                    image: <Icons.Dashboard />,
                    route: "/"
                },
                {
                    text: "Accounts",
                    image: <Icons.PiggyBank3 />,
                    route: "/accounts"
                },
                {
                    text: "Budget",
                    image: <Icons.Budget />,
                    route: "/budget"
                },
                {
                    text: "Groups",
                    image: <Icons.Stack />,
                    route: "/account-groups"
                },
                {
                    text: "Tags",
                    image: <Icons.Tags />,
                    route: "/tags"
                },
            ]} />
            <ErrorBoundary>
                <Routes>
                    <Route path="/" element={<Pages.Dashboard />} />
                    <Route path="/accounts" element={<Pages.Accounts />} />
                    <Route path="/accounts/manage" element={<Pages.ManageAccounts />} />
                    <Route path="/accounts/create" element={<Pages.CreateAccount />} />
                    <Route path="/accounts/:id" element={<Pages.Account />}>
                        <Route path="manage" element={<Pages.ManageAccount />} />
                        <Route path="transactions" element={<Pages.Transactions />} />
                        <Route path="transactions/add" element={<Pages.AddTransaction />} />
                        <Route path="manage/virtual/create" element={<Pages.CreateVirtualAccount />} />
                        <Route path="manage/virtual/:virtualId" element={<Pages.ManageVirtualAccount />} />
                        <Route path="rules" element={<Pages.Rules />} />
                        <Route path="import" element={<Pages.Import />} />
                        <Route path="reports" element={<Pages.Reports />}>
                            <Route path="in-out" element={<Pages.InOutPage />} />
                            <Route path="breakdown/:tagId?" element={<Pages.Breakdown />} />
                            <Route path="by-tag" element={<Pages.ByTag />} />
                            <Route path="tag-trend/:tagId?" element={<Pages.TagTrend />} />
                            <Route path="all-tag-average" element={<Pages.AllTagAverage />} />
                        </Route>
                        <Route path="virtual/:virtualId" element={<Pages.VirtualAccount />}>
                            <Route path="transactions" element={<Pages.Transactions />} />
                            <Route path="transactions/add" element={<Pages.AddTransaction />} />
                            <Route path="manage" element={<Pages.ManageVirtualAccount />} />
                        </Route>
                    </Route>
                    <Route path="/stock/create" element={<Pages.CreateStockHolding />} />
                    <Route path="/stock/:id" element={<Pages.StockHolding />}>
                        <Route path="manage" element={<Pages.ManageStockHolding />} />
                        <Route path="transactions" element={<Pages.StockTransactions />} />
                    </Route>
                    <Route path="/budget" element={<Pages.Budget />} />
                    <Route path="/budget/report/:year?/:month?" element={<Pages.BudgetReport />} />
                    <Route path="/settings" element={<Pages.Settings />}>
                        <Route path="families" element={<Pages.Families />} />
                        <Route path="families/add" element={<Pages.CreateFamily />} />
                        <Route path="institutions" element={<Pages.Institutions />} />
                        <Route path="institutions/add" element={<Pages.CreateInstitution />} />
                        <Route path="institutions/:id" element={<Pages.ManageInstitution />} />
                    </Route>
                    <Route path="/tags" element={<Pages.TransactionTags />} />
                    <Route path="/tags/visualiser" element={<Pages.Visualiser />} />
                    <Route path="/account-groups" element={<Pages.ManageAccountGroups />} />
                    <Route path="/account-groups/:id/manage" element={<Pages.ManageAccountGroup />} />
                    <Route path="/account-groups/create" element={<Pages.CreateAccountGroup />} />
                    <Route path="*" element={<Pages.Error404 />} />
                </Routes>
            </ErrorBoundary>
            <Layout.Footer copyrightYear={2013} />
        </Layout>
    );
};

export default App;
