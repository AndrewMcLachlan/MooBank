import { useMemo } from "react";
import * as Pages from "./pages";
import { RouteDefinition } from "@andrewmclachlan/mooapp";
import App from "App";

export const routes: RouteDefinition = {
    layout: {
        path: "/", element: <App />, children: {
            home: { path: "/", element: <Pages.Dashboard /> },
            accounts: { path: "/accounts", element: <Pages.Accounts /> },
            accountsManage: { path: "/accounts/manage", element: <Pages.ManageAccounts /> },
            accountsCreate: { path: "/accounts/create", element: <Pages.CreateAccount /> },
            account: {
                path: "/accounts/:id", element: <Pages.Account />, children: {
                    manage: { path: "manage", element: <Pages.ManageAccount /> },
                    transactions: { path: "transactions", element: <Pages.Transactions /> },
                    transactionsAdd: { path: "transactions/add", element: <Pages.AddTransaction /> },
                    balanceAdjustment: { path: "balance", element: <Pages.BalanceAdjustment /> },
                    virtualCreate: { path: "manage/virtual/create", element: <Pages.CreateVirtualAccount /> },
                    virtualManage: { path: "manage/virtual/:virtualId", element: <Pages.ManageVirtualAccount /> },
                    rules: { path: "rules", element: <Pages.Rules /> },
                    import: { path: "import", element: <Pages.Import /> },
                    reports: {
                        path: "reports", element: <Pages.Reports />, children: {
                            inout: { path: "in-out", element: <Pages.InOutPage /> },
                            breakdown: { path: "breakdown/:tagId?", element: <Pages.Breakdown /> },
                            byTag: { path: "by-tag", element: <Pages.ByTag /> },
                            tagTrend: { path: "tag-trend/:tagId?", element: <Pages.TagTrend /> },
                            allTagAverage: { path: "all-tag-average", element: <Pages.AllTagAverage /> },
                        }
                    },
                    virtual: {
                        path: "virtual/:virtualId", element: <Pages.VirtualAccount />, children: {
                            transactions: { path: "transactions", element: <Pages.Transactions /> },
                            transactionsAdd: { path: "transactions/add", element: <Pages.AddTransaction /> },
                            balanceAdjustment: { path: "balance", element: <Pages.BalanceAdjustment /> },
                            manage: { path: "manage", element: <Pages.ManageVirtualAccount /> },
                        }
                    },
                }
            },
            stockCreate: { path: "/shares/create", element: <Pages.CreateStockHolding /> },
            stock: {
                path: "/shares/:id", element: <Pages.StockHolding />, children: {
                    manage: { path: "manage", element: <Pages.ManageStockHolding /> },
                    transactions: { path: "transactions", element: <Pages.StockTransactions /> },
                }
            },
            assetCreate: { path: "/assets/create", element: <Pages.CreateAsset /> },
            asset: {
                path: "/assets/:id", element: <Pages.Asset />, children: {
                    manage: { path: "manage", element: <Pages.ManageAsset /> },
                }
            },
            budget: { path: "/budget", element: <Pages.Budget /> },
            budgetReport: { path: "/budget/report/:year?/:month?", element: <Pages.BudgetReport /> },
            settings: {
                path: "/settings", element: <Pages.Settings />, children: {
                    families: { path: "families", element: <Pages.Families /> },
                    familiesAdd: { path: "families/add", element: <Pages.CreateFamily /> },
                    institutions: { path: "institutions", element: <Pages.Institutions /> },
                    institutionsAdd: { path: "institutions/add", element: <Pages.CreateInstitution /> },
                    institution: { path: "institutions/:id", element: <Pages.ManageInstitution /> },
                }
            },
            tags: { path: "/tags", element: <Pages.TransactionTags /> },
            tagsVisualiser: { path: "/tags/visualiser", element: <Pages.Visualiser /> },
            accountGroups: { path: "/groups", element: <Pages.ManageAccountGroups /> },
            accountGroupsManage: { path: "/groups/:id/manage", element: <Pages.ManageAccountGroup /> },
            accountGroupsCreate: { path: "/groups/create", element: <Pages.CreateAccountGroup /> },
            profile: { path: "/profile", element: <Pages.Profile /> },
            fallback: { path: "*", element: <Pages.Error404 /> },
        }
    }
};
